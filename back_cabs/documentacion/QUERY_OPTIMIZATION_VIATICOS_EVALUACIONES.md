# Plan de Optimización - Viáticos y Evaluaciones

## 🎯 Objetivo

Optimizar queries en los módulos **GastoViatico** y **Evaluacion** que SÍ se mantendrán en producción, excluyendo módulos que serán eliminados (OrdenTrabajo, Cotizacion, Reparacion).

---

## 📊 Análisis Actual

### GastoViaticoRepository
- ✅ **AsNoTracking()** implementado
- ❌ **Falta Include()** para Detalles (si existe relación)
- ❌ **Falta propiedad de navegación** en modelo

### DetalleEvaluacionRepository  
- ❌ **Falta AsNoTracking()** en GetByEvaluacionIdAsync()
- ❌ **Falta Include()** para relaciones
- ❌ **Falta propiedad de navegación** en modelo Evaluacion

---

## 📝 Implementación

### Fase 1: Agregar Propiedades de Navegación a Modelos

#### 1.1 Modelo Evaluacion - Agregar Detalles y Fotos

**Archivo:** `back_cabs/crm/models/shared/Evaluaciones.cs`

```csharp
using System;
using System.Collections.Generic; // ✅ Agregar
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Shared
{
    [Table("evaluaciones")]
    public class Evaluacion
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("orden_id")]
        public int OrdenId { get; set; }

        [Column("ejecucion_id")]
        public int? EjecucionId { get; set; }

        [Column("cliente_id")]
        public int? ClienteId { get; set; }

        [Required]
        [Column("evaluador_id")]
        public int EvaluadorId { get; set; }

        [Column("objetivo")]
        [StringLength(200)]
        public string? Objetivo { get; set; }

        [Column("comentarios_generales")]
        public string? ComentariosGenerales { get; set; }

        [Column("score_calidad_total")]
        public int? ScoreCalidadTotal { get; set; }

        [Required]
        [Column("requiere_seguimiento")]
        public bool RequiereSeguimiento { get; set; } = false;

        [Column("seguimiento_notas")]
        public string? SeguimientoNotas { get; set; }

        [Required]
        [Column("creado_en", TypeName = "DATETIME2(0)")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // ✅ AGREGAR: Propiedades de navegación
        /// <summary>
        /// Detalles de la evaluación (relación 1:N)
        /// </summary>
        public virtual ICollection<EvaluacionDetalle> Detalles { get; set; } = new List<EvaluacionDetalle>();

        /// <summary>
        /// Fotos de la evaluación (relación 1:N)
        /// </summary>
        public virtual ICollection<EvaluacionFoto> Fotos { get; set; } = new List<EvaluacionFoto>();
    }
}
```

#### 1.2 Modelo GastoViatico - Agregar Detalles (si aplica)

**Archivo:** `back_cabs/crm/models/shared/GastoViatico.cs`

```csharp
using System;
using System.Collections.Generic; // ✅ Agregar si hay detalles
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

        [Column("orden_id")]
        public int? OrdenId { get; set; }

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

        // ✅ AGREGAR: Propiedad de navegación (si existe tabla de detalles)
        /// <summary>
        /// Detalles del gasto viático (si existe relación)
        /// </summary>
        public virtual ICollection<GastoViaticoDetalle>? Detalles { get; set; }
    }
}
```

---

### Fase 2: Optimizar DetalleEvaluacionRepository

**Archivo:** `back_cabs/crm/repositories/shared/IDetallesEvaluacionRepository.cs`

#### Cambio 1: GetByEvaluacionIdAsync() - Agregar AsNoTracking()

```csharp
// ❌ ANTES (Línea 22-27)
public async Task<List<EvaluacionDetalle>> GetByEvaluacionIdAsync(int evaluacionId)
{
    return await _readOnlyContext.EvaluacionesDetalles
                .Where(d => d.EvaluacionId == evaluacionId)
                .ToListAsync();
}

// ✅ DESPUÉS
public async Task<List<EvaluacionDetalle>> GetByEvaluacionIdAsync(int evaluacionId)
{
    return await _readOnlyContext.EvaluacionesDetalles
                .AsNoTracking() // ✅ Agregar
                .Where(d => d.EvaluacionId == evaluacionId)
                .OrderBy(d => d.Id) // ✅ Agregar ordenamiento
                .ToListAsync();
}
```

#### Cambio 2: GetByIdAsync() - Usar FirstOrDefaultAsync con AsNoTracking

```csharp
// ❌ ANTES (Línea 29-33)
public async Task<EvaluacionDetalle?> GetByIdAsync(int id)
{
    // Nota: Usamos ReadOnlyContext para lecturas
    return await _readOnlyContext.EvaluacionesDetalles.FindAsync(id);
}

// ✅ DESPUÉS
public async Task<EvaluacionDetalle?> GetByIdAsync(int id)
{
    return await _readOnlyContext.EvaluacionesDetalles
        .AsNoTracking() // ✅ Agregar
        .FirstOrDefaultAsync(d => d.Id == id);
}
```

---

### Fase 3: Crear Servicio de Evaluaciones con Include()

**Archivo:** `back_cabs/crm/services/shared/EvaluacionService.cs` (verificar si existe)

Si el servicio necesita cargar evaluaciones completas:

```csharp
public async Task<Evaluacion?> GetEvaluacionCompletaAsync(int id)
{
    return await _readContext.Evaluaciones
        .AsNoTracking()
        .Include(e => e.Detalles) // ✅ Carga detalles
        .Include(e => e.Fotos) // ✅ Carga fotos
        .FirstOrDefaultAsync(e => e.Id == id);
}

public async Task<List<Evaluacion>> GetEvaluacionesPorOrdenAsync(int ordenId)
{
    return await _readContext.Evaluaciones
        .AsNoTracking()
        .Include(e => e.Detalles) // ✅ Carga detalles
        .Include(e => e.Fotos) // ✅ Carga fotos
        .Where(e => e.OrdenId == ordenId)
        .OrderByDescending(e => e.CreadoEn)
        .ToListAsync();
}
```

---

### Fase 4: Optimizar GastoViaticoRepository (si tiene detalles)

**Archivo:** `back_cabs/crm/repositories/shared/IGastosVIaticosRepository.cs`

#### Cambio 1: GetViaticoByIdReadOnlyAsync() - Agregar Include si hay detalles

```csharp
// ❌ ANTES (Línea 35-40)
public async Task<GastoViatico?> GetViaticoByIdReadOnlyAsync(int id)
{
    return await _readContext.GastosViaticos
        .AsNoTracking()
        .FirstOrDefaultAsync(v => v.Id == id);
}

// ✅ DESPUÉS (solo si existe tabla de detalles)
public async Task<GastoViatico?> GetViaticoByIdReadOnlyAsync(int id)
{
    return await _readContext.GastosViaticos
        .AsNoTracking()
        .Include(v => v.Detalles) // ✅ Agregar si existe relación
        .FirstOrDefaultAsync(v => v.Id == id);
}
```

#### Cambio 2: GetViaticosFilteredAsync() - Agregar Include

```csharp
// ❌ ANTES (Línea 47-70)
public async Task<(List<GastoViatico> Items, int TotalCount)> GetViaticosFilteredAsync(
    int? ordenId, DateTime? fechaDesde, DateTime? fechaHasta, int pageNumber, int pageSize)
{
    var query = _readContext.GastosViaticos
        .AsNoTracking()
        .AsQueryable();

    if (ordenId.HasValue)
        query = query.Where(v => v.OrdenId == ordenId.Value);
    if (fechaDesde.HasValue)
        query = query.Where(v => v.Fecha >= fechaDesde.Value);
    if (fechaHasta.HasValue)
        query = query.Where(v => v.Fecha <= fechaHasta.Value);

    var totalCount = await query.CountAsync();

    var viaticos = await query
        .OrderByDescending(v => v.Fecha)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (viaticos, totalCount);
}

// ✅ DESPUÉS (solo si existe tabla de detalles)
public async Task<(List<GastoViatico> Items, int TotalCount)> GetViaticosFilteredAsync(
    int? ordenId, DateTime? fechaDesde, DateTime? fechaHasta, int pageNumber, int pageSize)
{
    var query = _readContext.GastosViaticos
        .AsNoTracking()
        .Include(v => v.Detalles) // ✅ Agregar si existe relación
        .AsQueryable();

    if (ordenId.HasValue)
        query = query.Where(v => v.OrdenId == ordenId.Value);
    if (fechaDesde.HasValue)
        query = query.Where(v => v.Fecha >= fechaDesde.Value);
    if (fechaHasta.HasValue)
        query = query.Where(v => v.Fecha <= fechaHasta.Value);

    var totalCount = await query.CountAsync();

    var viaticos = await query
        .OrderByDescending(v => v.Fecha)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (viaticos, totalCount);
}
```

---

### Fase 5: Índices de Base de Datos

```sql
-- =====================================================
-- ÍNDICES PARA EVALUACIONES Y VIÁTICOS
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🔍 Creando índices para Evaluaciones y Viáticos...';
GO

-- =====================================================
-- EVALUACIONES
-- =====================================================

-- Índice para búsquedas por OrdenId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evaluaciones_OrdenId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Evaluaciones_OrdenId
    ON Evaluaciones(OrdenId)
    INCLUDE (EvaluadorId, CreadoEn, ScoreCalidadTotal);
    PRINT '✅ Índice IX_Evaluaciones_OrdenId creado';
END
ELSE
    PRINT '⏭️  Índice IX_Evaluaciones_OrdenId ya existe';
GO

-- Índice para búsquedas por Evaluador
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evaluaciones_EvaluadorId_CreadoEn')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Evaluaciones_EvaluadorId_CreadoEn
    ON Evaluaciones(EvaluadorId, CreadoEn DESC)
    INCLUDE (OrdenId, ScoreCalidadTotal);
    PRINT '✅ Índice IX_Evaluaciones_EvaluadorId_CreadoEn creado';
END
ELSE
    PRINT '⏭️  Índice IX_Evaluaciones_EvaluadorId_CreadoEn ya existe';
GO

-- Índice para búsquedas por EjecucionId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evaluaciones_EjecucionId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Evaluaciones_EjecucionId
    ON Evaluaciones(EjecucionId)
    WHERE EjecucionId IS NOT NULL
    INCLUDE (OrdenId, EvaluadorId, CreadoEn);
    PRINT '✅ Índice IX_Evaluaciones_EjecucionId creado';
END
ELSE
    PRINT '⏭️  Índice IX_Evaluaciones_EjecucionId ya existe';
GO

-- =====================================================
-- EVALUACION DETALLES
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EvaluacionDetalles_EvaluacionId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_EvaluacionDetalles_EvaluacionId
    ON EvaluacionesDetalles(EvaluacionId)
    INCLUDE (Concepto, Calificacion, Observaciones);
    PRINT '✅ Índice IX_EvaluacionDetalles_EvaluacionId creado';
END
ELSE
    PRINT '⏭️  Índice IX_EvaluacionDetalles_EvaluacionId ya existe';
GO

-- =====================================================
-- EVALUACION FOTOS
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EvaluacionFotos_EvaluacionId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_EvaluacionFotos_EvaluacionId
    ON EvaluacionesFotos(EvaluacionId)
    INCLUDE (RutaArchivo, FechaSubida);
    PRINT '✅ Índice IX_EvaluacionFotos_EvaluacionId creado';
END
ELSE
    PRINT '⏭️  Índice IX_EvaluacionFotos_EvaluacionId ya existe';
GO

-- =====================================================
-- GASTOS VIÁTICOS
-- =====================================================

-- Índice para búsquedas por OrdenId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_OrdenId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_OrdenId
    ON finance_gastos_viaticos(orden_id)
    WHERE orden_id IS NOT NULL
    INCLUDE (fecha, monto_total, descripcion);
    PRINT '✅ Índice IX_GastosViaticos_OrdenId creado';
END
ELSE
    PRINT '⏭️  Índice IX_GastosViaticos_OrdenId ya existe';
GO

-- Índice para búsquedas por Fecha
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_Fecha')
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_Fecha
    ON finance_gastos_viaticos(fecha DESC)
    INCLUDE (orden_id, monto_total, tiene_factura);
    PRINT '✅ Índice IX_GastosViaticos_Fecha creado';
END
ELSE
    PRINT '⏭️  Índice IX_GastosViaticos_Fecha ya existe';
GO

-- Índice para búsquedas por rango de fechas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_FechaRango')
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_FechaRango
    ON finance_gastos_viaticos(fecha, orden_id)
    INCLUDE (monto_total);
    PRINT '✅ Índice IX_GastosViaticos_FechaRango creado';
END
ELSE
    PRINT '⏭️  Índice IX_GastosViaticos_FechaRango ya existe';
GO

-- =====================================================
-- GASTOS VIÁTICOS DETALLES (si existe)
-- =====================================================

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'finance_gastos_viaticos_detalles')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticosDetalles_GastoId')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_GastosViaticosDetalles_GastoId
        ON finance_gastos_viaticos_detalles(gasto_id)
        INCLUDE (concepto, monto);
        PRINT '✅ Índice IX_GastosViaticosDetalles_GastoId creado';
    END
    ELSE
        PRINT '⏭️  Índice IX_GastosViaticosDetalles_GastoId ya existe';
END
GO

PRINT '';
PRINT '✅ ¡Índices para Evaluaciones y Viáticos creados exitosamente!';
GO
```

---

## 🧪 Testing

### Test 1: Evaluaciones con Include()
```csharp
[Fact]
public async Task GetEvaluacionCompleta_DebeCargarDetallesYFotos()
{
    // Arrange
    var service = new EvaluacionService(_readContext, _logger);
    
    // Act
    var evaluacion = await service.GetEvaluacionCompletaAsync(1);
    
    // Assert
    Assert.NotNull(evaluacion);
    Assert.NotNull(evaluacion.Detalles); // ✅ Debe estar cargado
    Assert.NotNull(evaluacion.Fotos); // ✅ Debe estar cargado
}
```

### Test 2: DetalleEvaluacion con AsNoTracking()
```csharp
[Fact]
public async Task GetByEvaluacionIdAsync_DebeUsarAsNoTracking()
{
    // Arrange
    var repository = new DetalleEvaluacionRepository(_readContext, _writeContext);
    
    // Act
    var detalles = await repository.GetByEvaluacionIdAsync(1);
    
    // Assert
    Assert.NotNull(detalles);
    // Verificar que no están siendo tracked
    var trackedEntities = _readContext.ChangeTracker.Entries().Count();
    Assert.Equal(0, trackedEntities);
}
```

---

## 📊 Impacto Esperado

### Antes
- **Evaluaciones**: 2-3 queries (N+1 con Detalles y Fotos)
- **Viáticos**: 1 query (sin detalles)
- **Tiempo promedio**: 100-200ms

### Después
- **Evaluaciones**: 1 query (con JOINs)
- **Viáticos**: 1 query (con JOIN si hay detalles)
- **Tiempo promedio**: 30-60ms (50-70% mejora)

---

## ✅ Checklist

- [ ] **Backup de BD**
- [ ] **Crear rama** `feature/optimize-viaticos-evaluaciones`
- [ ] **Modelos**
  - [ ] Agregar propiedades navegación en Evaluacion
  - [ ] Agregar propiedades navegación en GastoViatico (si aplica)
- [ ] **Repositorios**
  - [ ] Optimizar DetalleEvaluacionRepository
  - [ ] Optimizar GastoViaticoRepository
  - [ ] Crear/actualizar EvaluacionService con Include()
- [ ] **Índices**
  - [ ] Ejecutar script SQL en DEV
  - [ ] Verificar creación de índices
- [ ] **Tests**
  - [ ] Tests unitarios
  - [ ] Tests de performance
- [ ] **Deploy**
  - [ ] Code review
  - [ ] Merge a develop
  - [ ] Deploy a staging
  - [ ] Verificación
  - [ ] Deploy a producción

---

**Siguiente paso:** ¿Procedo con la implementación?
