# Plan de Migración - Refactorización Tabla Viáticos

## 🎯 Objetivos

1. ✅ Hacer `orden_id` **NULLABLE** (viáticos independientes de órdenes)
2. ✅ Agregar `vehiculo_id` **NULLABLE** (tracking de vehículo usado)
3. ✅ Agregar campo `disponible` a tabla `fleet_vehiculos`
4. ✅ Implementar lógica de control de disponibilidad

---

## 📊 Cambios en Base de Datos

### Migración 1: Modificar `finance_gastos_viaticos`

```sql
-- =====================================================
-- MIGRACIÓN: Refactorización Tabla Viáticos
-- Fecha: 2025-12-10
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🔄 Iniciando migración de finance_gastos_viaticos...';
GO

-- =====================================================
-- PASO 1: Eliminar FK existente de orden_id
-- =====================================================

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_gastos_viaticos_orden')
BEGIN
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    DROP CONSTRAINT [FK_gastos_viaticos_orden];
    PRINT '  ✅ FK_gastos_viaticos_orden eliminada';
END
GO

-- =====================================================
-- PASO 2: Hacer orden_id NULLABLE
-- =====================================================

ALTER TABLE [dbo].[finance_gastos_viaticos]
ALTER COLUMN [orden_id] [int] NULL;
PRINT '  ✅ orden_id ahora es NULLABLE';
GO

-- =====================================================
-- PASO 3: Agregar columna vehiculo_id
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('finance_gastos_viaticos') AND name = 'vehiculo_id')
BEGIN
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    ADD [vehiculo_id] [int] NULL;
    PRINT '  ✅ Columna vehiculo_id agregada';
END
GO

-- =====================================================
-- PASO 4: Agregar FK a fleet_vehiculos
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_gastos_viaticos_vehiculo')
BEGIN
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    ADD CONSTRAINT [FK_gastos_viaticos_vehiculo] FOREIGN KEY([vehiculo_id])
    REFERENCES [dbo].[fleet_vehiculos] ([id]);
    PRINT '  ✅ FK_gastos_viaticos_vehiculo creada';
END
GO

-- =====================================================
-- PASO 5: Recrear FK de orden_id (ahora nullable)
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_gastos_viaticos_orden')
BEGIN
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    WITH NOCHECK ADD CONSTRAINT [FK_gastos_viaticos_orden] FOREIGN KEY([orden_id])
    REFERENCES [dbo].[ops_ordenes_trabajo] ([id])
    ON DELETE CASCADE;
    
    ALTER TABLE [dbo].[finance_gastos_viaticos] 
    NOCHECK CONSTRAINT [FK_gastos_viaticos_orden];
    
    PRINT '  ✅ FK_gastos_viaticos_orden recreada (nullable)';
END
GO

PRINT '';
PRINT '✅ Migración de finance_gastos_viaticos completada';
GO
```

---

### Migración 2: Modificar `fleet_vehiculos`

```sql
-- =====================================================
-- MIGRACIÓN: Agregar campo disponible a fleet_vehiculos
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🔄 Iniciando migración de fleet_vehiculos...';
GO

-- =====================================================
-- PASO 1: Agregar columna disponible
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('fleet_vehiculos') AND name = 'disponible')
BEGIN
    ALTER TABLE [dbo].[fleet_vehiculos]
    ADD [disponible] [bit] NOT NULL DEFAULT 1;
    PRINT '  ✅ Columna disponible agregada (default: 1 = disponible)';
END
GO

-- =====================================================
-- PASO 2: Actualizar vehículos existentes
-- =====================================================

-- Marcar como NO disponibles los vehículos actualmente en uso
UPDATE v
SET v.disponible = 0
FROM [dbo].[fleet_vehiculos] v
INNER JOIN [dbo].[fleet_uso_vehiculos] u ON v.id = u.vehiculo_id
WHERE u.estado = 'EN_USO';

PRINT '  ✅ Vehículos en uso marcados como no disponibles';
GO

PRINT '';
PRINT '✅ Migración de fleet_vehiculos completada';
GO
```

---

### Migración 3: Crear Índices Actualizados

```sql
-- =====================================================
-- ÍNDICES ACTUALIZADOS PARA VIÁTICOS
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🔍 Creando índices actualizados...';
GO

-- Índice para búsquedas por VehiculoId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_VehiculoId' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_VehiculoId
    ON finance_gastos_viaticos(vehiculo_id)
    WHERE vehiculo_id IS NOT NULL
    INCLUDE (fecha, monto_total, lugar_destino, km_recorridos);
    PRINT '  ✅ Índice IX_GastosViaticos_VehiculoId creado';
END
GO

-- Índice para vehículos disponibles
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculos_Disponible' AND object_id = OBJECT_ID('fleet_vehiculos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Vehiculos_Disponible
    ON fleet_vehiculos(disponible, activo)
    WHERE disponible = 1 AND activo = 1
    INCLUDE (nombre_vehiculo, placas, tipo_vehiculo);
    PRINT '  ✅ Índice IX_Vehiculos_Disponible creado';
END
GO

PRINT '';
PRINT '✅ Índices creados exitosamente';
GO
```

---

## 📝 Cambios en Modelos C#

### Modelo GastoViatico

**Archivo:** `back_cabs/crm/models/shared/GastoViatico.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Shared
{
    [Table("finance_gastos_viaticos")]
    public class GastoViatico
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ✅ MODIFICADO: Ahora es nullable
        [Column("orden_id")]
        public int? OrdenId { get; set; }

        // ✅ NUEVO: Vehículo usado (opcional)
        [Column("vehiculo_id")]
        public int? VehiculoId { get; set; }

        [Required]
        [Column("tiene_factura")]
        public bool TieneFactura { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("proveedor_nombre")]
        [StringLength(200)]
        public string? ProveedorNombre { get; set; }

        [Required]
        [Column("fecha", TypeName = "date")]
        public DateTime Fecha { get; set; }

        [Column("km_recorridos")]
        public int? KmRecorridos { get; set; }

        [Required]
        [Column("gastos")]
        [StringLength(200)]
        public string Gastos { get; set; } = string.Empty;

        [Required]
        [Column("monto_total", TypeName = "decimal(12, 2)")]
        public decimal MontoTotal { get; set; }

        [Column("lugar_destino")]
        [StringLength(200)]
        public string? LugarDestino { get; set; }

        // ✅ NUEVO: Propiedad de navegación
        /// <summary>
        /// Vehículo utilizado para el viático (opcional)
        /// </summary>
        [ForeignKey("VehiculoId")]
        public virtual Vehiculo? Vehiculo { get; set; }
    }
}
```

---

### Modelo Vehiculo

**Archivo:** `back_cabs/crm/models/shared/Vehiculo.cs`

```csharp
// AGREGAR campo disponible

[Required]
[Column("disponible")]
public bool Disponible { get; set; } = true;
```

---

## 🔧 Cambios en Repository

### GastoViaticoRepository

**Archivo:** `back_cabs/crm/repositories/shared/IGastosVIaticosRepository.cs`

```csharp
// AGREGAR método para cargar con vehículo

public async Task<GastoViatico?> GetViaticoConVehiculoAsync(int id)
{
    return await _readContext.GastosViaticos
        .AsNoTracking()
        .Include(v => v.Vehiculo) // ✅ Cargar vehículo si existe
        .FirstOrDefaultAsync(v => v.Id == id);
}
```

---

## 🎯 Lógica de Negocio - Control de Disponibilidad

### Servicio de Viáticos

```csharp
public class GastoViaticoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GastoViaticoService> _logger;

    public async Task<GastoViatico> CrearViaticoAsync(CrearViaticoDto dto)
    {
        // Validar y marcar vehículo como no disponible si se usa
        if (dto.VehiculoId.HasValue)
        {
            var vehiculo = await _unitOfWork.VehiculoRepository
                .GetByIdAsync(dto.VehiculoId.Value);

            if (vehiculo == null)
                throw new NotFoundException("Vehículo no encontrado");

            if (!vehiculo.Disponible)
                throw new BusinessException("El vehículo no está disponible");

            // ✅ Marcar como no disponible
            vehiculo.Disponible = false;
            await _unitOfWork.VehiculoRepository.UpdateAsync(vehiculo);
        }

        var viatico = new GastoViatico
        {
            OrdenId = dto.OrdenId, // Puede ser null
            VehiculoId = dto.VehiculoId,
            TieneFactura = dto.TieneFactura,
            Descripcion = dto.Descripcion,
            ProveedorNombre = dto.ProveedorNombre,
            Fecha = dto.Fecha,
            KmRecorridos = dto.KmRecorridos,
            Gastos = dto.Gastos,
            MontoTotal = dto.MontoTotal,
            LugarDestino = dto.LugarDestino
        };

        await _unitOfWork.GastoViaticoRepository.CreateAsync(viatico);
        await _unitOfWork.SaveChangesAsync();

        return viatico;
    }

    public async Task FinalizarViaticoAsync(int viaticoId)
    {
        var viatico = await _unitOfWork.GastoViaticoRepository
            .GetByIdAsync(viaticoId);

        if (viatico == null)
            throw new NotFoundException("Viático no encontrado");

        // ✅ Liberar vehículo si se usó
        if (viatico.VehiculoId.HasValue)
        {
            var vehiculo = await _unitOfWork.VehiculoRepository
                .GetByIdAsync(viatico.VehiculoId.Value);

            if (vehiculo != null)
            {
                vehiculo.Disponible = true;
                await _unitOfWork.VehiculoRepository.UpdateAsync(vehiculo);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation(
                    "Vehículo {VehiculoId} liberado tras finalizar viático {ViaticoId}",
                    vehiculo.Id, viaticoId);
            }
        }
    }
}
```

---

## 📊 DTOs Actualizados

```csharp
public class CrearViaticoDto
{
    public int? OrdenId { get; set; } // ✅ Ahora opcional
    public int? VehiculoId { get; set; } // ✅ Nuevo
    public bool TieneFactura { get; set; }
    public string? Descripcion { get; set; }
    public string? ProveedorNombre { get; set; }
    
    [Required]
    public DateTime Fecha { get; set; }
    
    public int? KmRecorridos { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Gastos { get; set; } = string.Empty;
    
    [Required]
    public decimal MontoTotal { get; set; }
    
    [StringLength(200)]
    public string? LugarDestino { get; set; }
}

public class ViaticoResponseDto
{
    public int Id { get; set; }
    public int? OrdenId { get; set; }
    public int? VehiculoId { get; set; }
    public string? VehiculoNombre { get; set; } // ✅ Nuevo
    public string? VehiculoPlacas { get; set; } // ✅ Nuevo
    public bool TieneFactura { get; set; }
    public string? Descripcion { get; set; }
    public string? ProveedorNombre { get; set; }
    public DateTime Fecha { get; set; }
    public int? KmRecorridos { get; set; }
    public string Gastos { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    public string? LugarDestino { get; set; }
}
```

---

## ✅ Checklist de Implementación

### Fase 1: Base de Datos
- [ ] **Backup de base de datos**
- [ ] Ejecutar Migración 1 (finance_gastos_viaticos)
- [ ] Ejecutar Migración 2 (fleet_vehiculos)
- [ ] Ejecutar Migración 3 (índices)
- [ ] Verificar estructura de tablas

### Fase 2: Modelos
- [ ] Actualizar GastoViatico.cs
- [ ] Actualizar Vehiculo.cs
- [ ] Compilar y verificar

### Fase 3: Repository
- [ ] Actualizar GastoViaticoRepository
- [ ] Agregar método GetViaticoConVehiculoAsync
- [ ] Compilar y verificar

### Fase 4: Servicios y DTOs
- [ ] Crear/actualizar GastoViaticoService
- [ ] Actualizar DTOs
- [ ] Implementar lógica de disponibilidad
- [ ] Compilar y verificar

### Fase 5: Verificación
- [ ] Tests unitarios
- [ ] Pruebas de integración
- [ ] Verificar control de disponibilidad
- [ ] Verificar viáticos sin orden

---

## 🔄 Plan de Rollback

Si algo falla:

```sql
-- Revertir cambios en finance_gastos_viaticos
ALTER TABLE finance_gastos_viaticos DROP CONSTRAINT FK_gastos_viaticos_vehiculo;
ALTER TABLE finance_gastos_viaticos DROP COLUMN vehiculo_id;
ALTER TABLE finance_gastos_viaticos ALTER COLUMN orden_id int NOT NULL;

-- Revertir cambios en fleet_vehiculos
ALTER TABLE fleet_vehiculos DROP COLUMN disponible;
```

---

## 📝 Notas Importantes

1. **Datos Existentes:** Todos los viáticos existentes mantendrán su `orden_id`
2. **Nuevos Viáticos:** Pueden crearse sin `orden_id` (independientes)
3. **Vehículos:** Control automático de disponibilidad
4. **Compatibilidad:** Cambios retrocompatibles con código existente

---

**Siguiente paso:** ¿Procedo con la implementación de las migraciones?
