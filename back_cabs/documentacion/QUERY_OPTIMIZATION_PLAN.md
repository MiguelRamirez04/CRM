# Plan de Optimización de Queries - Enfoque Práctico

## 🎯 Repositorios a Optimizar (Excluyendo OrdenTrabajo)

### ✅ Repositorios YA Optimizados
- `EjecucionOrdenRepository` - Include() y AsNoTracking() ✅
- `VehiculoRepository` - AsNoTracking() consistente ✅  
- `AdmClienteRepository` - AsNoTracking() ✅
- `AdmProductoRepository` - AsNoTracking() ✅

### ⚠️ Repositorios que NECESITAN Optimización

1. **CotizacionRepository** - Falta Include() para Items
2. **ReparacionRepository** - Falta Include() para Componentes y Fotos
3. **Índices de Base de Datos** - Faltan índices críticos

--

## 📝 Implementación

### 1. CotizacionRepository - Agregar Include() para Items

#### ❌ Problema Actual
```csharp
// Línea 54-56: GetByIdAsync sin Include
public async Task<Cotizacion?> GetByIdAsync(int id)
{
    var cotizacion = await _readContext.Cotizaciones
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == id);
    // ❌ Si accedes a cotizacion.Items, generará query adicional (N+1)
}
```

#### ✅ Solución
```csharp
public async Task<Cotizacion?> GetByIdAsync(int id)
{
    var cotizacion = await _readContext.Cotizaciones
        .AsNoTracking()
        .Include(c => c.Items) // ✅ Carga Items en una sola query
        .FirstOrDefaultAsync(c => c.Id == id);
    
    return cotizacion;
}
```

#### Métodos a Modificar en CotizacionRepository

**1. GetAllAsync() - Línea 35**
```csharp
public async Task<IEnumerable<Cotizacion>> GetAllAsync()
{
    var cotizaciones = await _readContext.Cotizaciones
        .AsNoTracking()
        .Include(c => c.Items) // ✅ Agregar
        .OrderByDescending(c => c.CreadoEn)
        .ToListAsync();
    
    return cotizaciones;
}
```

**2. GetByOrdenIdAsync() - Línea 80**
```csharp
public async Task<IEnumerable<Cotizacion>> GetByOrdenIdAsync(int ordenId)
{
    var cotizaciones = await _readContext.Cotizaciones
        .AsNoTracking()
        .Include(c => c.Items) // ✅ Agregar
        .Where(c => c.OrdenId == ordenId)
        .OrderByDescending(c => c.CreadoEn)
        .ToListAsync();
    
    return cotizaciones;
}
```

**3. GetByEstadoAsync() - Línea 100**
```csharp
public async Task<IEnumerable<Cotizacion>> GetByEstadoAsync(string estado)
{
    var cotizaciones = await _readContext.Cotizaciones
        .AsNoTracking()
        .Include(c => c.Items) // ✅ Agregar
        .Where(c => c.Estado == estado)
        .OrderByDescending(c => c.CreadoEn)
        .ToListAsync();
    
    return cotizaciones;
}
```

---

### 2. ReparacionRepository - Agregar Include() para Relaciones

#### ❌ Problema Actual
```csharp
// Línea 89-91: ObtenerReparacionPorIdAsync sin Include
public async Task<Reparacion?> ObtenerReparacionPorIdAsync(int id)
{
    return await _readContext.Reparaciones
        .AsNoTracking()
        .FirstOrDefaultAsync(r => r.Id == id);
    // ❌ Si accedes a r.Componentes o r.Orden, generará queries adicionales
}
```

#### ✅ Solución
```csharp
public async Task<Reparacion?> ObtenerReparacionPorIdAsync(int id)
{
    return await _readContext.Reparaciones
        .AsNoTracking()
        .Include(r => r.Orden) // ✅ Carga Orden
        .Include(r => r.Tecnico) // ✅ Carga Técnico
        .Include(r => r.Componentes) // ✅ Carga Componentes
            .ThenInclude(c => c.Fotos) // ✅ Carga Fotos de cada componente
        .FirstOrDefaultAsync(r => r.Id == id);
}
```

#### Métodos a Modificar en ReparacionRepository

**1. ObtenerReparacionesAsync() - Línea 64**
```csharp
public async Task<IEnumerable<Reparacion>> ObtenerReparacionesAsync(int? skip, int? take)
{
    var query = _readContext.Reparaciones
        .AsNoTracking()
        .Include(r => r.Orden) // ✅ Agregar
        .Include(r => r.Tecnico) // ✅ Agregar
        .AsQueryable();

    if (skip.HasValue) query = query.Skip(skip.Value);
    if (take.HasValue) query = query.Take(take.Value);

    return await query 
        .OrderByDescending(r => r.FechaLlegada)
        .ToListAsync();
}
```

**2. ObtenerComponentesReparacionAsync() - Línea 207**
```csharp
public async Task<IEnumerable<ReparacionComponente>> ObtenerComponentesReparacionAsync(int? skip, int? take)
{
    var query = _readContext.ReparacionesComponentes
        .AsNoTracking()
        .Include(c => c.Reparacion) // ✅ Agregar si necesitas datos de la reparación
        .Include(c => c.Fotos) // ✅ Agregar para cargar fotos
        .AsQueryable();

    if (skip.HasValue) query = query.Skip(skip.Value);
    if (take.HasValue) query = query.Take(take.Value);

    return await query.OrderByDescending(r => r.Id).ToListAsync();
}
```

**3. ObtenerComponenteReparacionPorIdAsync() - Línea 228**
```csharp
public async Task<ReparacionComponente?> ObtenerComponenteReparacionPorIdAsync(int id)
{
    return await _readContext.ReparacionesComponentes
        .AsNoTracking()
        .Include(c => c.Reparacion) // ✅ Agregar
        .Include(c => c.Fotos) // ✅ Agregar
        .FirstOrDefaultAsync(r => r.Id == id);
}
```

---

### 3. Índices de Base de Datos

#### Script SQL Optimizado
```sql
-- =====================================================
-- ÍNDICES CRÍTICOS PARA OPTIMIZACIÓN
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🔍 Verificando e instalando índices...';
GO

-- =====================================================
-- COTIZACIONES
-- =====================================================

-- Índice para búsquedas por OrdenId (muy frecuente)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Cotizaciones_OrdenId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Cotizaciones_OrdenId
    ON Cotizaciones(OrdenId)
    INCLUDE (Estado, CreadoEn, Total, Cliente);
    PRINT '✅ Índice IX_Cotizaciones_OrdenId creado';
END
ELSE
    PRINT '⏭️  Índice IX_Cotizaciones_OrdenId ya existe';
GO

-- Índice para búsquedas por Estado
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Cotizaciones_Estado_CreadoEn')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Cotizaciones_Estado_CreadoEn
    ON Cotizaciones(Estado, CreadoEn DESC)
    INCLUDE (OrdenId, Cliente, Total);
    PRINT '✅ Índice IX_Cotizaciones_Estado_CreadoEn creado';
END
ELSE
    PRINT '⏭️  Índice IX_Cotizaciones_Estado_CreadoEn ya existe';
GO

-- Índice para búsquedas por fecha
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Cotizaciones_CreadoEn')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Cotizaciones_CreadoEn
    ON Cotizaciones(CreadoEn DESC)
    INCLUDE (Estado, OrdenId, Total);
    PRINT '✅ Índice IX_Cotizaciones_CreadoEn creado';
END
ELSE
    PRINT '⏭️  Índice IX_Cotizaciones_CreadoEn ya existe';
GO

-- =====================================================
-- COTIZACION ITEMS (para Include)
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CotizacionItems_CotizacionId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_CotizacionItems_CotizacionId
    ON CotizacionItems(CotizacionId)
    INCLUDE (Descripcion, Cantidad, PrecioUnitario, Total);
    PRINT '✅ Índice IX_CotizacionItems_CotizacionId creado';
END
ELSE
    PRINT '⏭️  Índice IX_CotizacionItems_CotizacionId ya existe';
GO

-- =====================================================
-- REPARACIONES
-- =====================================================

-- Índice para búsquedas por OrdenId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reparaciones_OrdenId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Reparaciones_OrdenId
    ON Reparaciones(OrdenId)
    INCLUDE (TecnicoId, Estado, FechaLlegada);
    PRINT '✅ Índice IX_Reparaciones_OrdenId creado';
END
ELSE
    PRINT '⏭️  Índice IX_Reparaciones_OrdenId ya existe';
GO

-- Índice para búsquedas por Técnico
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reparaciones_TecnicoId_Estado')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Reparaciones_TecnicoId_Estado
    ON Reparaciones(TecnicoId, Estado)
    INCLUDE (OrdenId, FechaLlegada);
    PRINT '✅ Índice IX_Reparaciones_TecnicoId_Estado creado';
END
ELSE
    PRINT '⏭️  Índice IX_Reparaciones_TecnicoId_Estado ya existe';
GO

-- Índice para ordenamiento por fecha
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reparaciones_FechaLlegada')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Reparaciones_FechaLlegada
    ON Reparaciones(FechaLlegada DESC)
    INCLUDE (OrdenId, TecnicoId, Estado);
    PRINT '✅ Índice IX_Reparaciones_FechaLlegada creado';
END
ELSE
    PRINT '⏭️  Índice IX_Reparaciones_FechaLlegada ya existe';
GO

-- =====================================================
-- REPARACION COMPONENTES
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReparacionComponentes_ReparacionId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_ReparacionComponentes_ReparacionId
    ON ReparacionComponentes(ReparacionId)
    INCLUDE (Nombre, Descripcion, Estado);
    PRINT '✅ Índice IX_ReparacionComponentes_ReparacionId creado';
END
ELSE
    PRINT '⏭️  Índice IX_ReparacionComponentes_ReparacionId ya existe';
GO

-- =====================================================
-- REPARACION FOTOS
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReparacionFotos_ComponenteId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_ReparacionFotos_ComponenteId
    ON ReparacionFotos(ComponenteId)
    INCLUDE (DocumentoId, FechaSubida);
    PRINT '✅ Índice IX_ReparacionFotos_ComponenteId creado';
END
ELSE
    PRINT '⏭️  Índice IX_ReparacionFotos_ComponenteId ya existe';
GO

-- =====================================================
-- EJECUCION ORDEN (ya optimizado pero agregar índices)
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EjecucionOrden_TecnicoId_HrInicio')
BEGIN
    CREATE NONCLUSTERED INDEX IX_EjecucionOrden_TecnicoId_HrInicio
    ON EjecucionOrden(TecnicoId, HrInicio DESC)
    INCLUDE (OrdenId, VehiculoId, Estado);
    PRINT '✅ Índice IX_EjecucionOrden_TecnicoId_HrInicio creado';
END
ELSE
    PRINT '⏭️  Índice IX_EjecucionOrden_TecnicoId_HrInicio ya existe';
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EjecucionOrden_OrdenId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_EjecucionOrden_OrdenId
    ON EjecucionOrden(OrdenId)
    INCLUDE (TecnicoId, VehiculoId, HrInicio, HrFin);
    PRINT '✅ Índice IX_EjecucionOrden_OrdenId creado';
END
ELSE
    PRINT '⏭️  Índice IX_EjecucionOrden_OrdenId ya existe';
GO

-- =====================================================
-- VEHICULOS
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculos_Activo_Placas')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Vehiculos_Activo_Placas
    ON Vehiculos(Activo, Placas)
    INCLUDE (Marca, Modelo, Año);
    PRINT '✅ Índice IX_Vehiculos_Activo_Placas creado';
END
ELSE
    PRINT '⏭️  Índice IX_Vehiculos_Activo_Placas ya existe';
GO

PRINT '';
PRINT '✅ ¡Todos los índices verificados/creados exitosamente!';
PRINT '📊 Ejecuta DBCC SHOW_STATISTICS para verificar uso de índices';
GO
```

---

## 🧪 Testing

### 1. Test de Include() - CotizacionRepository
```csharp
[Fact]
public async Task GetByIdAsync_DebeCargarItems()
{
    // Arrange
    var repository = new CotizacionRepository(_writeContext, _readContext, _logger);
    
    // Act
    var cotizacion = await repository.GetByIdAsync(1);
    
    // Assert
    Assert.NotNull(cotizacion);
    Assert.NotNull(cotizacion.Items); // ✅ Debe estar cargado
    Assert.True(cotizacion.Items.Count > 0); // ✅ Debe tener items
}
```

### 2. Test de Include() - ReparacionRepository
```csharp
[Fact]
public async Task ObtenerReparacionPorIdAsync_DebeCargarComponentesYFotos()
{
    // Arrange
    var repository = new ReparacionRepository(_writeContext, _readContext, _logger);
    
    // Act
    var reparacion = await repository.ObtenerReparacionPorIdAsync(1);
    
    // Assert
    Assert.NotNull(reparacion);
    Assert.NotNull(reparacion.Orden); // ✅ Debe estar cargado
    Assert.NotNull(reparacion.Tecnico); // ✅ Debe estar cargado
    Assert.NotNull(reparacion.Componentes); // ✅ Debe estar cargado
    
    if (reparacion.Componentes.Any())
    {
        var primerComponente = reparacion.Componentes.First();
        Assert.NotNull(primerComponente.Fotos); // ✅ Fotos deben estar cargadas
    }
}
```

### 3. Test de Performance
```csharp
[Fact]
public async Task GetByIdAsync_DebeTomar MenosDe100ms()
{
    // Arrange
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    var cotizacion = await _repository.GetByIdAsync(1);
    stopwatch.Stop();
    
    // Assert
    Assert.True(stopwatch.ElapsedMilliseconds < 100, 
        $"Query tomó {stopwatch.ElapsedMilliseconds}ms (esperado <100ms)");
}
```

---

## 📊 Impacto Esperado

### Antes de Optimización
- **CotizacionRepository.GetByIdAsync()**: 2-3 queries (N+1 con Items)
- **ReparacionRepository.ObtenerReparacionPorIdAsync()**: 3-5 queries (N+1 con Componentes y Fotos)
- **Tiempo promedio**: 150-300ms

### Después de Optimización
- **CotizacionRepository.GetByIdAsync()**: 1 query (con JOIN)
- **ReparacionRepository.ObtenerReparacionPorIdAsync()**: 1 query (con JOINs)
- **Tiempo promedio**: 40-80ms (60-70% mejora)

---

## ✅ Checklist de Implementación

- [ ] **Backup de base de datos**
- [ ] **Crear rama** `feature/query-optimization-phase1`
- [ ] **CotizacionRepository**
  - [ ] Modificar GetByIdAsync()
  - [ ] Modificar GetAllAsync()
  - [ ] Modificar GetByOrdenIdAsync()
  - [ ] Modificar GetByEstadoAsync()
- [ ] **ReparacionRepository**
  - [ ] Modificar ObtenerReparacionPorIdAsync()
  - [ ] Modificar ObtenerReparacionesAsync()
  - [ ] Modificar ObtenerComponentesReparacionAsync()
  - [ ] Modificar ObtenerComponenteReparacionPorIdAsync()
- [ ] **Ejecutar script de índices** en DEV
- [ ] **Tests unitarios**
- [ ] **Tests de performance**
- [ ] **Code review**
- [ ] **Merge a develop**
- [ ] **Deploy a staging**
- [ ] **Verificación en staging**
- [ ] **Deploy a producción**

---

## 🔄 Plan de Rollback

Si algo falla:

```bash
# Revertir cambios en código
git revert <commit-hash>

# Eliminar índices si causan problemas
DROP INDEX IX_Cotizaciones_OrdenId ON Cotizaciones;
DROP INDEX IX_Reparaciones_OrdenId ON Reparaciones;
# ... etc
```

---

**Siguiente paso:** ¿Procedo con la implementación de CotizacionRepository?
