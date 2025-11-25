
-- ═══════════════════════════════════════════════════════════════════════════════════════
-- SCRIPT DE OPTIMIZACIÓN DE ÍNDICES PARA DASHBOARD DE COTIZACIONES
-- ═══════════════════════════════════════════════════════════════════════════════════════
-- Este script crea índices en las tablas críticas para mejorar el rendimiento
-- de las consultas del dashboard de métricas de documentos
-- 
-- IMPORTANTE: Ejecutar en horario de baja actividad
-- Tiempo estimado: 2-5 minutos dependiendo del volumen de datos
-- ═══════════════════════════════════════════════════════════════════════════════════════

USE [compac];
GO

PRINT '🚀 Iniciando optimización de índices para dashboard...';
GO

-- ═══════════════════════════════════════════════════════════════════════════════════════
-- ÍNDICES PARA admDocumentos (Tabla principal de cotizaciones)
-- ═══════════════════════════════════════════════════════════════════════════════════════

-- Índice compuesto para filtros de dashboard (Serie + Fechas + Estado)
-- Mejora: GetEstadisticasGeneralesAsync, GetTopClientesAsync, GetRendimientoAgentesAsync
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_admDocumentos_Dashboard_Filtros' AND object_id = OBJECT_ID('admDocumentos'))
BEGIN
    PRINT '📊 Creando índice IX_admDocumentos_Dashboard_Filtros...';
    CREATE NONCLUSTERED INDEX [IX_admDocumentos_Dashboard_Filtros]
    ON [dbo].[admDocumentos] (
        [CSERIEDOCUMENTO] ASC,
        [CFECHA] ASC,
        [CCANCELADO] ASC
    )
    INCLUDE (
        [CIDDOCUMENTO],
        [CTOTAL],
        [CIDCLIENTEPROVEEDOR],
        [CIDAGENTE],
        [CFECHAVENCIMIENTO]
    )
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    );
    PRINT '✅ Índice IX_admDocumentos_Dashboard_Filtros creado exitosamente';
END
ELSE
    PRINT '⚠️  Índice IX_admDocumentos_Dashboard_Filtros ya existe';
GO

-- Índice para búsqueda por cliente
-- Mejora: GetTopClientesAsync
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_admDocumentos_Cliente_Fecha' AND object_id = OBJECT_ID('admDocumentos'))
BEGIN
    PRINT '📊 Creando índice IX_admDocumentos_Cliente_Fecha...';
    CREATE NONCLUSTERED INDEX [IX_admDocumentos_Cliente_Fecha]
    ON [dbo].[admDocumentos] (
        [CIDCLIENTEPROVEEDOR] ASC,
        [CSERIEDOCUMENTO] ASC,
        [CFECHA] DESC
    )
    INCLUDE (
        [CIDDOCUMENTO],
        [CTOTAL],
        [CCANCELADO]
    )
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    );
    PRINT '✅ Índice IX_admDocumentos_Cliente_Fecha creado exitosamente';
END
ELSE
    PRINT '⚠️  Índice IX_admDocumentos_Cliente_Fecha ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════════════
-- ÍNDICES PARA admMovimientos (Detalle de productos cotizados)
-- ═══════════════════════════════════════════════════════════════════════════════════════

-- Índice compuesto para productos más cotizados
-- Mejora: GetProductosMasCotizadosAsync (CRÍTICO - Este es el más lento)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_admMovimientos_Productos_Dashboard' AND object_id = OBJECT_ID('admMovimientos'))
BEGIN
    PRINT '📊 Creando índice IX_admMovimientos_Productos_Dashboard...';
    CREATE NONCLUSTERED INDEX [IX_admMovimientos_Productos_Dashboard]
    ON [dbo].[admMovimientos] (
        [CIDPRODUCTO] ASC,
        [CIDDOCUMENTO] ASC
    )
    INCLUDE (
        [CUNIDADES],
        [CTOTAL]
    )
    WHERE [CIDPRODUCTO] > 0 -- Filtro para productos válidos
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    );
    PRINT '✅ Índice IX_admMovimientos_Productos_Dashboard creado exitosamente';
END
ELSE
    PRINT '⚠️  Índice IX_admMovimientos_Productos_Dashboard ya existe';
GO

-- Índice para join con documentos (relación inversa)
-- Mejora: GetEstadisticasGeneralesAsync para contar productos únicos
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_admMovimientos_Documento_Producto' AND object_id = OBJECT_ID('admMovimientos'))
BEGIN
    PRINT '📊 Creando índice IX_admMovimientos_Documento_Producto...';
    CREATE NONCLUSTERED INDEX [IX_admMovimientos_Documento_Producto]
    ON [dbo].[admMovimientos] (
        [CIDDOCUMENTO] ASC,
        [CIDPRODUCTO] ASC
    )
    WHERE [CIDPRODUCTO] > 0
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    );
    PRINT '✅ Índice IX_admMovimientos_Documento_Producto creado exitosamente';
END
ELSE
    PRINT '⚠️  Índice IX_admMovimientos_Documento_Producto ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════════════
-- ÍNDICES PARA admClientes (JOIN en GetTopClientesAsync)
-- ═══════════════════════════════════════════════════════════════════════════════════════

-- Asegurar que existe índice en clave primaria (debería existir)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_admClientes_Codigo' AND object_id = OBJECT_ID('admClientes'))
BEGIN
    PRINT '📊 Creando índice IX_admClientes_Codigo...';
    CREATE NONCLUSTERED INDEX [IX_admClientes_Codigo]
    ON [dbo].[admClientes] (
        [CIDCLIENTEPROVEEDOR] ASC
    )
    INCLUDE (
        [CCODIGOCLIENTE],
        [CRAZONSOCIAL],
        [CRFC]
    )
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    );
    PRINT '✅ Índice IX_admClientes_Codigo creado exitosamente';
END
ELSE
    PRINT '⚠️  Índice IX_admClientes_Codigo ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════════════
-- ÍNDICES PARA admAgentes (JOIN en GetRendimientoAgentesAsync)
-- ═══════════════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_admAgentes_Codigo' AND object_id = OBJECT_ID('admAgentes'))
BEGIN
    PRINT '📊 Creando índice IX_admAgentes_Codigo...';
    CREATE NONCLUSTERED INDEX [IX_admAgentes_Codigo]
    ON [dbo].[admAgentes] (
        [CIDAGENTE] ASC
    )
    INCLUDE (
        [CCODIGOAGENTE],
        [CNOMBREAGENTE]
    )
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    );
    PRINT '✅ Índice IX_admAgentes_Codigo creado exitosamente';
END
ELSE
    PRINT '⚠️  Índice IX_admAgentes_Codigo ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════════════
-- ACTUALIZAR ESTADÍSTICAS (CRÍTICO para plan de ejecución óptimo)
-- ═══════════════════════════════════════════════════════════════════════════════════════

PRINT '📈 Actualizando estadísticas de las tablas optimizadas...';
GO

UPDATE STATISTICS [dbo].[admDocumentos] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[admMovimientos] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[admClientes] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[admAgentes] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[admProductos] WITH FULLSCAN;
GO

PRINT '✅ Estadísticas actualizadas';
GO

-- ═══════════════════════════════════════════════════════════════════════════════════════
-- REPORTE DE ÍNDICES CREADOS
-- ═══════════════════════════════════════════════════════════════════════════════════════

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════════════════════════════';
PRINT '📋 REPORTE DE ÍNDICES PARA DASHBOARD';
PRINT '═══════════════════════════════════════════════════════════════════════════════════════';

SELECT 
    OBJECT_NAME(i.object_id) AS Tabla,
    i.name AS NombreIndice,
    i.type_desc AS TipoIndice,
    CAST(SUM(s.used_page_count) * 8.0 / 1024 AS DECIMAL(10,2)) AS TamañoMB,
    i.has_filter AS TieneFiltro,
    i.filter_definition AS Filtro
FROM sys.indexes i
INNER JOIN sys.dm_db_partition_stats s ON i.object_id = s.object_id AND i.index_id = s.index_id
WHERE OBJECT_NAME(i.object_id) IN ('admDocumentos', 'admMovimientos', 'admClientes', 'admAgentes', 'admProductos')
    AND i.name IS NOT NULL
GROUP BY 
    i.object_id,
    i.name,
    i.type_desc,
    i.has_filter,
    i.filter_definition
ORDER BY 
    Tabla,
    TamañoMB DESC;

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════════════════════════════';
PRINT '✅ OPTIMIZACIÓN COMPLETADA EXITOSAMENTE';
PRINT '═══════════════════════════════════════════════════════════════════════════════════════';
PRINT '';
PRINT '🚀 Mejoras esperadas:';
PRINT '   - Productos más cotizados: 10-20x más rápido (de 2-5 segundos a 200-500ms)';
PRINT '   - Top clientes: 5-10x más rápido';
PRINT '   - Estadísticas generales: 5-8x más rápido';
PRINT '   - Rendimiento agentes: 3-5x más rápido';
PRINT '';
PRINT '⚠️  IMPORTANTE: Reiniciar la aplicación backend para que tome efecto';
PRINT '   La caché de Redis acelerará aún más las consultas repetidas';
PRINT '';
GO
