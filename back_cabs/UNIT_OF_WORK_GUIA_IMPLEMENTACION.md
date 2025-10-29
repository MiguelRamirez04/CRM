# ✅ UNIT OF WORK PATTERN - GUÍA DE IMPLEMENTACIÓN Y REFACTORIZACIÓN

## 📋 ÍNDICE
1. [¿Qué es Unit of Work?](#qué-es-unit-of-work)
2. [¿Cuándo usar Unit of Work?](#cuándo-usar-unit-of-work)
3. [Estructura implementada](#estructura-implementada)
4. [Ejemplo de refactorización](#ejemplo-de-refactorización)
5. [Casos de uso comunes](#casos-de-uso-comunes)
6. [Testing y verificación](#testing-y-verificación)

---

## 🎯 ¿Qué es Unit of Work?

El patrón **Unit of Work** coordina múltiples repositorios bajo una **transacción única**, garantizando que todas las operaciones se completen exitosamente o ninguna se aplique (atomicidad ACID).

### Analogía del mundo real
Imagina una transferencia bancaria:
- ❌ **SIN Unit of Work**: Deducir dinero de cuenta A → ❌ Error al añadir a cuenta B = **Dinero perdido**
- ✅ **CON Unit of Work**: Deducir + Añadir en transacción → ❌ Error = **Rollback automático**

---

## 🚦 ¿Cuándo usar Unit of Work?

### ✅ USA Unit of Work cuando:
1. **Múltiples operaciones deben completarse juntas**
   ```
   Crear Orden + Crear Ejecución + Actualizar Vehículo = 1 transacción
   ```

2. **Necesitas garantizar consistencia de datos**
   ```
   Si falla la evaluación, revertir la creación de fotos
   ```

3. **Operaciones críticas de negocio**
   ```
   Finalizar orden + Generar factura + Actualizar inventario
   ```

### ❌ NO uses Unit of Work cuando:
1. **Operaciones de solo lectura** (usa Repository directo)
   ```csharp
   var vehiculos = await _vehiculoRepository.GetAllAsync();
   ```

2. **Operación única simple**
   ```csharp
   var orden = await _ordenRepository.CreateAsync(orden);
   ```

3. **No hay riesgo de inconsistencia**
   ```csharp
   await _logger.LogAsync("Acción completada");
   ```

---

## 🏗️ Estructura Implementada

### 📂 Archivos creados

```
back_cabs/
└── CRM/
    └── Core/
        └── UnitOfWork/
            ├── IUnitOfWork.cs          # Interfaz con contratos
            └── UnitOfWork.cs           # Implementación concreta
```

### 📝 IUnitOfWork.cs - Interfaz

```csharp
public interface IUnitOfWork : IDisposable
{
    // Repositorios disponibles
    IEjecucionOrdenRepository EjecucionOrden { get; }
    IOrdenTrabajoRepository OrdenTrabajo { get; }
    ICotizacionRepository Cotizaciones { get; }
    IUsuarioAuthRepository Usuarios { get; }
    IVehiculoRepository Vehiculos { get; }
    IReparacionRepository Reparaciones { get; }

    // Operaciones de persistencia
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    // Manejo de transacciones
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    bool HasActiveTransaction { get; }

    // Utilidades avanzadas
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    Task ExecuteInTransactionAsync(Func<Task> operation);
}
```

### ⚙️ Registro en DI Container

**Program.cs**
```csharp
// ═══════════════════════════════════════════════════════════════
// UNIT OF WORK PATTERN
// ═══════════════════════════════════════════════════════════════
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

---

## 🔄 Ejemplo de Refactorización

### ❌ ANTES - SIN Unit of Work

```csharp
// EjecucionOrdenService.cs - Versión antigua con transacciones manuales
public class EjecucionOrdenService
{
    private readonly IEjecucionOrdenRepository _ejecucionRepository;
    private readonly WriteContext _writeContext; // ❌ Contexto inyectado directamente
    private readonly ILogger<EjecucionOrdenService> _logger;

    public EjecucionOrdenService(
        IEjecucionOrdenRepository ejecucionRepository,
        WriteContext writeContext,
        ILogger<EjecucionOrdenService> logger)
    {
        _ejecucionRepository = ejecucionRepository;
        _writeContext = writeContext;
        _logger = logger;
    }

    public async Task<EjecucionOrdenResponseDto> CreateEjecucionAsync(EjecucionOrdenCreateRequestDto dto)
    {
        // ❌ Estrategia de ejecución manual
        var strategy = _writeContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // ❌ Transacción manual
            using var transaction = await _writeContext.Database.BeginTransactionAsync();
            try
            {
                // Crear ejecución
                var ejecucion = new EjecucionOrden { /* ... */ };
                var ejecucionCreada = await _ejecucionRepository.CreateAsync(ejecucion);

                // ❌ Commit manual
                await transaction.CommitAsync();

                return MapToResponseDto(ejecucionCreada);
            }
            catch (Exception ex)
            {
                // ❌ Rollback manual
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear ejecución");
                throw;
            }
        });
    }
}
```

### ✅ DESPUÉS - CON Unit of Work

```csharp
// EjecucionOrdenService.cs - Versión refactorizada con Unit of Work
using back_cabs.CRM.Core.UnitOfWork;

public class EjecucionOrdenService
{
    private readonly IUnitOfWork _unitOfWork; // ✅ Unit of Work en lugar de contexto
    private readonly ReadOnlyContext _readContext;
    private readonly ILogger<EjecucionOrdenService> _logger;

    public EjecucionOrdenService(
        IUnitOfWork unitOfWork, // ✅ Inyección de Unit of Work
        ReadOnlyContext readContext,
        ILogger<EjecucionOrdenService> logger)
    {
        _unitOfWork = unitOfWork;
        _readContext = readContext;
        _logger = logger;
    }

    public async Task<EjecucionOrdenResponseDto> CreateEjecucionAsync(EjecucionOrdenCreateRequestDto dto)
    {
        // ✅ Método 1: Transacción automática simplificada
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Validaciones con ReadContext (sin transacción)
            var orden = await _readContext.OrdenesTrabajo
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == dto.OrdenId);
            
            if (orden == null)
                throw new ArgumentException($"Orden {dto.OrdenId} no existe.");

            // Crear ejecución usando repositorio del Unit of Work
            var ejecucion = new EjecucionOrden
            {
                OrdenId = dto.OrdenId,
                TecnicoId = dto.TecnicoId,
                HrInicio = dto.HrInicio ?? DateTime.Now,
                // ...
            };

            // ✅ Acceso a repositorio a través de Unit of Work
            var ejecucionCreada = await _unitOfWork.EjecucionOrden.CreateAsync(ejecucion);

            // ✅ Commit automático al salir del bloque sin excepciones
            return MapToResponseDto(ejecucionCreada);
        });
    }

    // ✅ Método 2: Control manual (para casos complejos)
    public async Task<EjecucionOrdenResponseDto> CreateEjecucionManualAsync(EjecucionOrdenCreateRequestDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Validaciones y operaciones
            var ejecucion = new EjecucionOrden { /* ... */ };
            var ejecucionCreada = await _unitOfWork.EjecucionOrden.CreateAsync(ejecucion);

            // Operaciones adicionales si es necesario
            // await _unitOfWork.OrdenTrabajo.UpdateAsync(...);

            await _unitOfWork.CommitAsync(); // ✅ Commit explícito

            return MapToResponseDto(ejecucionCreada);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(); // ✅ Rollback explícito
            _logger.LogError(ex, "Error al crear ejecución");
            throw;
        }
    }
}
```

---

## 📚 Casos de Uso Comunes

### 1️⃣ Crear Orden con Ejecución (Operación Compleja)

```csharp
public async Task<OrdenTrabajoResponseDto> CrearOrdenConEjecucionAsync(OrdenConEjecucionDto dto)
{
    return await _unitOfWork.ExecuteInTransactionAsync(async () =>
    {
        // Paso 1: Crear orden de trabajo
        var orden = new OrdenTrabajo
        {
            ClienteId = dto.ClienteId,
            VehiculoId = dto.VehiculoId,
            Descripcion = dto.Descripcion,
            // ...
        };
        var ordenCreada = await _unitOfWork.OrdenTrabajo.CreateAsync(orden);

        // Paso 2: Crear ejecución asociada
        var ejecucion = new EjecucionOrden
        {
            OrdenId = ordenCreada.Id,
            TecnicoId = dto.TecnicoId,
            TipoEjecucion = dto.TipoEjecucion,
            // ...
        };
        await _unitOfWork.EjecucionOrden.CreateAsync(ejecucion);

        // Paso 3: Actualizar estado del vehículo
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(dto.VehiculoId);
        vehiculo.EstadoServicio = EstadoServicio.EN_TALLER;
        await _unitOfWork.Vehiculos.UpdateAsync(vehiculo);

        // ✅ Si cualquier paso falla, TODO se revierte automáticamente
        return MapToOrdenResponseDto(ordenCreada);
    });
}
```

### 2️⃣ Finalizar Ejecución con Actualización de Orden

```csharp
public async Task FinalizarEjecucionAsync(int ejecucionId, FinalizarEjecucionDto dto)
{
    await _unitOfWork.ExecuteInTransactionAsync(async () =>
    {
        // Paso 1: Actualizar ejecución
        var ejecucion = await _unitOfWork.EjecucionOrden.GetByIdAsync(ejecucionId);
        if (ejecucion == null)
            throw new KeyNotFoundException("Ejecución no encontrada");

        ejecucion.HrFin = DateTime.Now;
        ejecucion.KmFinal = dto.KmFinal;
        ejecucion.Comentarios = dto.Comentarios;
        await _unitOfWork.EjecucionOrden.UpdateAsync(ejecucion);

        // Paso 2: Actualizar estado de la orden
        var orden = await _unitOfWork.OrdenTrabajo.GetByIdAsync(ejecucion.OrdenId);
        orden.Estado = EstadoOrden.COMPLETADA;
        await _unitOfWork.OrdenTrabajo.UpdateAsync(orden);

        // Paso 3: Liberar vehículo
        if (ejecucion.VehiculoId.HasValue)
        {
            var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(ejecucion.VehiculoId.Value);
            vehiculo.EstadoServicio = EstadoServicio.DISPONIBLE;
            await _unitOfWork.Vehiculos.UpdateAsync(vehiculo);
        }

        // ✅ Todo se guarda en una sola transacción
    });
}
```

### 3️⃣ Transferir Ejecución entre Técnicos

```csharp
public async Task TransferirEjecucionAsync(int ejecucionId, int nuevoTecnicoId)
{
    await _unitOfWork.ExecuteInTransactionAsync(async () =>
    {
        // Paso 1: Obtener ejecución actual
        var ejecucion = await _unitOfWork.EjecucionOrden.GetByIdAsync(ejecucionId);
        if (ejecucion == null)
            throw new KeyNotFoundException("Ejecución no encontrada");

        var tecnicoAnterior = ejecucion.TecnicoId;

        // Paso 2: Actualizar técnico asignado
        ejecucion.TecnicoId = nuevoTecnicoId;
        ejecucion.Comentarios += $"\n[{DateTime.Now:yyyy-MM-dd HH:mm}] Transferido de técnico {tecnicoAnterior} a {nuevoTecnicoId}";
        await _unitOfWork.EjecucionOrden.UpdateAsync(ejecucion);

        // Paso 3: Registrar en historial (si tienes tabla de historial)
        // var historial = new HistorialAsignacion { ... };
        // await _unitOfWork.Historial.CreateAsync(historial);

        // ✅ Si falla cualquier paso, la transferencia se cancela completamente
    });
}
```

---

## 🧪 Testing y Verificación

### 1. Verificar compilación

```powershell
cd c:\Users\adria\source\repos\fullstack_cabs\back_cabs
dotnet build
```

**Salida esperada:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 2. Probar transacciones con Postman

**Test 1: Crear orden con ejecución**
```http
POST https://localhost:7264/api/ordenes-trabajo/con-ejecucion
Content-Type: application/json
Authorization: Bearer <tu-token>

{
  "clienteId": 1,
  "vehiculoId": 5,
  "descripcion": "Mantenimiento preventivo",
  "tecnicoId": 3,
  "tipoEjecucion": "CAMPO"
}
```

**Test 2: Simular error para verificar rollback**
```http
POST https://localhost:7264/api/ordenes-trabajo/con-ejecucion
Content-Type: application/json
Authorization: Bearer <tu-token>

{
  "clienteId": 1,
  "vehiculoId": 9999,  // ❌ Vehículo inexistente = debe revertir TODO
  "descripcion": "Test rollback",
  "tecnicoId": 3,
  "tipoEjecucion": "CAMPO"
}
```

**Resultado esperado:**
- ❌ Error 400: "Vehículo no encontrado"
- ✅ NO debe crear la orden (rollback automático)

### 3. Verificar en SQL Server

```sql
-- Verificar que NO se creó la orden del test fallido
SELECT TOP 5 * FROM OrdenesTrabajo 
ORDER BY Id DESC;

-- Verificar que NO se creó ejecución huérfana
SELECT e.* 
FROM EjecucionesOrden e
LEFT JOIN OrdenesTrabajo o ON e.OrdenId = o.Id
WHERE o.Id IS NULL; -- No debe devolver filas
```

---

## ✅ Checklist de Implementación Completada

- [x] **IUnitOfWork.cs** creado con 6 repositorios y métodos de transacción
- [x] **UnitOfWork.cs** implementado con lazy loading y gestión de transacciones
- [x] **Program.cs** actualizado con registro en DI container
- [x] **Documentación** completa con ejemplos prácticos

---

## 🔜 Siguientes Pasos

### 1. Refactorizar servicios existentes (recomendado)
   - `EjecucionOrdenService.CreateEjecucionAsync()`
   - `OrdenTrabajoService.CrearOrdenConEjecucionAsync()`
   - `ReparacionService` (operaciones complejas)

### 2. Implementar nuevas operaciones complejas
   - Finalizar orden + generar reporte + enviar email
   - Transferencia masiva de ejecuciones
   - Cierre de mes con múltiples actualizaciones

### 3. Testing avanzado
   - Unit tests con mocks de IUnitOfWork
   - Integration tests con base de datos de prueba
   - Pruebas de concurrencia y deadlocks

---

## 📖 Referencias y Recursos

- [Martin Fowler - Unit of Work Pattern](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [Microsoft - EF Core Transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions)
- [SOLID Principles - Dependency Inversion](https://en.wikipedia.org/wiki/Dependency_inversion_principle)

---

## 🎉 Mejoras Completadas

### ✅ Mejora 1: PasswordHash no expuesto (Verificado)
- DTOs validados sin campos sensibles

### ✅ Mejora 2: JWT Cookie Flags (Implementado)
- `CookieHelper.cs` con HttpOnly=true, Secure=true, SameSite=Strict

### ✅ Mejora 5: CSRF Protection (Implementado)
- `CsrfValidationMiddleware.cs` con validación de tokens
- Endpoint `/api/auth/csrf-token` para obtener token

### ✅ Mejora 6: Unit of Work Pattern (Implementado)
- `IUnitOfWork` + `UnitOfWork` con manejo de transacciones
- Registrado en DI container
- Documentación completa

---

**¡Unit of Work implementado exitosamente! 🚀**

Ahora tienes las herramientas para garantizar **consistencia de datos** en operaciones complejas.
