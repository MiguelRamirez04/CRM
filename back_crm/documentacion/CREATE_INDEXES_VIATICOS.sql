-- =====================================================
-- ÍNDICES PARA MÓDULO DE VIÁTICOS
-- Script de optimización de queries
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🔍 Iniciando creación de índices para módulo Viáticos...';
PRINT '';
GO

-- =====================================================
-- TABLA: finance_gastos_viaticos
-- =====================================================

PRINT '📊 Procesando tabla: finance_gastos_viaticos';
GO

-- Índice para búsquedas por OrdenId (muy frecuente)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_OrdenId' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_OrdenId
    ON finance_gastos_viaticos(orden_id)
    INCLUDE (fecha, monto_total, tiene_factura, lugar_destino, descripcion);
    PRINT '  ✅ Índice IX_GastosViaticos_OrdenId creado';
END
ELSE
    PRINT '  ⏭️  Índice IX_GastosViaticos_OrdenId ya existe';
GO

-- Índice para búsquedas por Fecha (reportes y filtros)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_Fecha' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_Fecha
    ON finance_gastos_viaticos(fecha DESC)
    INCLUDE (orden_id, monto_total, tiene_factura, lugar_destino);
    PRINT '  ✅ Índice IX_GastosViaticos_Fecha creado';
END
ELSE
    PRINT '  ⏭️  Índice IX_GastosViaticos_Fecha ya existe';
GO

-- Índice compuesto para rango de fechas (filtro más común en GetViaticosFilteredAsync)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_FechaRango' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_FechaRango
    ON finance_gastos_viaticos(fecha, orden_id)
    INCLUDE (monto_total, lugar_destino, tiene_factura, km_recorridos);
    PRINT '  ✅ Índice IX_GastosViaticos_FechaRango creado';
END
ELSE
    PRINT '  ⏭️  Índice IX_GastosViaticos_FechaRango ya existe';
GO

-- Índice para búsquedas por factura (control fiscal y reportes)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_TieneFactura' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_TieneFactura
    ON finance_gastos_viaticos(tiene_factura, fecha DESC)
    INCLUDE (orden_id, monto_total, proveedor_nombre, descripcion);
    PRINT '  ✅ Índice IX_GastosViaticos_TieneFactura creado';
END
ELSE
    PRINT '  ⏭️  Índice IX_GastosViaticos_TieneFactura ya existe';
GO

-- Índice para búsquedas por lugar destino (reportes por ciudad/estado)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_LugarDestino' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_LugarDestino
    ON finance_gastos_viaticos(lugar_destino, fecha DESC)
    WHERE lugar_destino IS NOT NULL
    INCLUDE (monto_total, km_recorridos, orden_id);
    PRINT '  ✅ Índice IX_GastosViaticos_LugarDestino creado (filtrado)';
END
ELSE
    PRINT '  ⏭️  Índice IX_GastosViaticos_LugarDestino ya existe';
GO

PRINT '';
PRINT '✅ ¡Todos los índices para Viáticos creados/verificados exitosamente!';
PRINT '';
PRINT '📊 Resumen de índices creados:';
PRINT '   - finance_gastos_viaticos: 5 índices';
PRINT '';
PRINT '💡 Recomendaciones:';
PRINT '   1. Ejecuta UPDATE STATISTICS para actualizar estadísticas';
PRINT '   2. Monitorea el uso de índices con sys.dm_db_index_usage_stats';
PRINT '   3. Considera reorganizar/reconstruir índices mensualmente';
PRINT '';
GO

-- =====================================================
-- VERIFICACIÓN OPCIONAL: Mostrar índices creados
-- =====================================================

PRINT '📋 Índices en tabla finance_gastos_viaticos:';
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    STUFF((
        SELECT ', ' + c.name
        FROM sys.index_columns ic
        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
        ORDER BY ic.key_ordinal
        FOR XML PATH('')
    ), 1, 2, '') AS KeyColumns,
    STUFF((
        SELECT ', ' + c.name
        FROM sys.index_columns ic
        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1
        FOR XML PATH('')
    ), 1, 2, '') AS IncludedColumns
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('finance_gastos_viaticos')
  AND i.name LIKE 'IX_%'
ORDER BY i.name;
GO

PRINT '';
PRINT '✅ Script completado exitosamente';
GO
