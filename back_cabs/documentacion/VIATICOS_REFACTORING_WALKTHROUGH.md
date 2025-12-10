# Refactorización Módulo Viáticos - Walkthrough

## 📊 Resumen

Se refactorizó el módulo de Viáticos para:
1. ✅ Permitir viáticos independientes de órdenes de trabajo
2. ✅ Agregar tracking de vehículos usados
3. ✅ Implementar control de disponibilidad de vehículos

---

## 🔄 Cambios Realizados

### 1. Base de Datos - Tabla `finance_gastos_viaticos`

**Archivo:** `MIGRATION_001_Viaticos_RefactorTable.sql`

**Cambios:**
- ✅ `orden_id` → **NULLABLE** (antes NOT NULL)
- ✅ Agregado campo `vehiculo_id` INT NULL
- ✅ FK a `fleet_vehiculos`
- ✅ FK `orden_id` recreada como nullable

**Antes:**
```sql
[orden_id] [int] NOT NULL,  -- Obligatorio
```

**Después:**
```sql
[orden_id] [int] NULL,      -- Opcional
[vehiculo_id] [int] NULL,   -- Nuevo campo
```

---

### 2. Base de Datos - Tabla `fleet_vehiculos`

**Archivo:** `MIGRATION_002_Vehiculos_AddDisponible.sql`

**Cambios:**
- ✅ Agregado campo `disponible` BIT NOT NULL DEFAULT 1
- ✅ Vehículos en uso marcados como no disponibles

**Nuevo Campo:**
```sql
[disponible] [bit] NOT NULL DEFAULT 1
```

**Lógica:**
- `disponible = 1` → Vehículo disponible para asignar
- `disponible = 0` → Vehículo en uso (en viático activo)

---

### 3. Índices Actualizados

**Archivo:** `MIGRATION_003_Indexes_Updated.sql`

**Nuevos Índices:**

#### finance_gastos_viaticos
1. `IX_GastosViaticos_VehiculoId` - Para búsquedas por vehículo
2. `IX_GastosViaticos_OrdenId` - Actualizado con filtro WHERE
3. `IX_GastosViaticos_Fecha` - Incluye vehiculo_id

#### fleet_vehiculos
1. `IX_Vehiculos_Disponible` - Para listar vehículos disponibles (filtrado)

---

### 4. Modelo GastoViatico

**Archivo:** `back_cabs/crm/models/shared/GastoViatico.cs`

**Cambios:**

```csharp
// ✅ MODIFICADO: Ahora es nullable
[Column("orden_id")]
public int? OrdenId { get; set; }

// ✅ NUEVO: Vehículo usado
[Column("vehiculo_id")]
public int? VehiculoId { get; set; }

// ✅ NUEVO: Propiedad de navegación
[ForeignKey("VehiculoId")]
public virtual Vehiculo? Vehiculo { get; set; }
```

---

### 5. Modelo Vehiculo

**Archivo:** `back_cabs/crm/models/shared/Vehiculo.cs`

**Cambios:**

```csharp
// ✅ NUEVO: Control de disponibilidad
[Required]
[Column("disponible")]
public bool Disponible { get; set; } = true;
```

---

### 6. GastoViaticoRepository

**Archivo:** `back_cabs/crm/repositories/shared/IGastosVIaticosRepository.cs`

**Nuevo Método:**

```csharp
/// <summary>
/// Obtiene un viático por ID incluyendo el vehículo relacionado
/// </summary>
public async Task<GastoViatico?> GetViaticoConVehiculoAsync(int id)
{
    return await _readContext.GastosViaticos
        .AsNoTracking()
        .Include(v => v.Vehiculo) // ✅ Eager loading
        .FirstOrDefaultAsync(v => v.Id == id);
}
```

---

## 📊 Casos de Uso

### Caso 1: Viático SIN Orden (Independiente)

```csharp
var viatico = new GastoViatico
{
    OrdenId = null, // ✅ Ahora es válido
    VehiculoId = 5,
    Descripcion = "Viaje a Monterrey",
    Fecha = DateTime.Today,
    MontoTotal = 1500.00m,
    LugarDestino = "Monterrey, NL"
};
```

### Caso 2: Viático CON Orden

```csharp
var viatico = new GastoViatico
{
    OrdenId = 123, // ✅ Sigue siendo válido
    VehiculoId = 5,
    Descripcion = "Servicio en sitio cliente",
    Fecha = DateTime.Today,
    MontoTotal = 800.00m
};
```

### Caso 3: Viático SIN Vehículo

```csharp
var viatico = new GastoViatico
{
    OrdenId = null,
    VehiculoId = null, // ✅ No se usó vehículo
    Descripcion = "Taxi al aeropuerto",
    Fecha = DateTime.Today,
    MontoTotal = 250.00m
};
```

---

## 🎯 Lógica de Negocio Recomendada

### Crear Viático con Vehículo

```csharp
public async Task<GastoViatico> CrearViaticoAsync(CrearViaticoDto dto)
{
    // Validar y marcar vehículo como no disponible
    if (dto.VehiculoId.HasValue)
    {
        var vehiculo = await _vehiculoRepository.GetByIdAsync(dto.VehiculoId.Value);
        
        if (vehiculo == null)
            throw new NotFoundException("Vehículo no encontrado");
        
        if (!vehiculo.Disponible)
            throw new BusinessException("El vehículo no está disponible");
        
        // ✅ Marcar como no disponible
        vehiculo.Disponible = false;
        await _vehiculoRepository.UpdateAsync(vehiculo);
    }
    
    var viatico = new GastoViatico { /* ... */ };
    await _viaticoRepository.CreateAsync(viatico);
    
    return viatico;
}
```

### Finalizar Viático (Liberar Vehículo)

```csharp
public async Task FinalizarViaticoAsync(int viaticoId)
{
    var viatico = await _viaticoRepository.GetViaticoConVehiculoAsync(viaticoId);
    
    // ✅ Liberar vehículo si se usó
    if (viatico?.VehiculoId.HasValue == true && viatico.Vehiculo != null)
    {
        viatico.Vehiculo.Disponible = true;
        await _vehiculoRepository.UpdateAsync(viatico.Vehiculo);
    }
}
```

---

## ✅ Verificación

### 1. Ejecutar Migraciones SQL

```sql
-- En SQL Server Management Studio
USE CABS_Pruebas;
GO

-- Ejecutar en orden:
:r MIGRATION_001_Viaticos_RefactorTable.sql
:r MIGRATION_002_Vehiculos_AddDisponible.sql
:r MIGRATION_003_Indexes_Updated.sql
```

### 2. Verificar Estructura de Tablas

```sql
-- Verificar finance_gastos_viaticos
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('finance_gastos_viaticos')
  AND c.name IN ('orden_id', 'vehiculo_id');

-- Verificar fleet_vehiculos
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('fleet_vehiculos')
  AND c.name = 'disponible';
```

### 3. Verificar FKs

```sql
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'finance_gastos_viaticos';
```

### 4. Compilar Proyecto

```powershell
cd c:\Users\ANA\Documents\dev\FullStack_CABS\back_cabs
dotnet build
```

---

## 📊 Impacto

### Antes de Refactorización
- ❌ Viáticos SIEMPRE requieren orden de trabajo
- ❌ No hay tracking de vehículos usados
- ❌ No hay control de disponibilidad de vehículos
- ❌ Posible asignación de vehículo ya en uso

### Después de Refactorización
- ✅ Viáticos pueden ser independientes
- ✅ Tracking completo de vehículos usados
- ✅ Control automático de disponibilidad
- ✅ Prevención de doble asignación
- ✅ Mejor reporting (viáticos por vehículo)

---

## 🔄 Compatibilidad con Datos Existentes

- ✅ **Viáticos existentes:** Mantienen su `orden_id` (no se afectan)
- ✅ **Vehículos existentes:** Se marcan como disponibles por default
- ✅ **Código existente:** Sigue funcionando (cambios retrocompatibles)

---

## 📝 Próximos Pasos Recomendados

1. **Actualizar DTOs** en controllers
2. **Actualizar Frontend** para permitir viáticos sin orden
3. **Implementar UI** para selección de vehículos disponibles
4. **Agregar validaciones** en controllers
5. **Crear reportes** de viáticos por vehículo

---

**Fecha:** 2025-12-10  
**Módulo:** Viáticos  
**Tipo:** Refactorización de esquema y lógica de negocio
