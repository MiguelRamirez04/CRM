-- =====================================================
-- ÍNDICES PARA MÓDULO DE EVALUACIONES
-- Script de optimización de queries
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🔍 Iniciando creación de índices para módulo Evaluaciones...';
PRINT '';
GO

-- =====================================================
-- TABLA: evaluaciones
-- =====================================================

PRINT '📊 Procesando tabla: evaluaciones';
GO

-- Índice para búsquedas por OrdenId (muy frecuente)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evaluaciones_OrdenId' AND object_id = OBJECT_ID('evaluaciones'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Evaluaciones_OrdenId
    ON evaluaciones(orden_id)
    INCLUDE (evaluador_id, creado_en, score_calidad_total, requiere_seguimiento);
    PRINT '  ✅ Índice IX_Evaluaciones_OrdenId creado';
END
ELSE
    PRINT '  ⏭️  Índice IX_Evaluaciones_OrdenId ya existe';
GO

-- Índice para búsquedas por Evaluador con ordenamiento por fecha
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evaluaciones_EvaluadorId_CreadoEn' AND object_id = OBJECT_ID('evaluaciones'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Evaluaciones_EvaluadorId_CreadoEn
    ON evaluaciones(evaluador_id, creado_en DESC)
    INCLUDE (orden_id, score_calidad_total, requiere_seguimiento);
    PRINT '  ✅ Índice IX_Evaluaciones_EvaluadorId_CreadoEn creado';
END
ELSE
    PRINT '  ⏭️  Índice IX_Evaluaciones_EvaluadorId_CreadoEn ya existe';
GO

-- Índice para búsquedas por EjecucionId (con filtro WHERE para valores no nulos)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evaluaciones_EjecucionId' AND object_id = OBJECT_ID('evaluaciones'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Evaluaciones_EjecucionId
    ON evaluaciones(ejecucion_id)
    WHERE ejecucion_id IS NOT NULL
    INCLUDE (orden_id, evaluador_id, creado_en);
    PRINT '  ✅ Índice IX_Evaluaciones_EjecucionId creado (filtrado)';
END
ELSE
    PRINT '  ⏭️  Índice IX_Evaluaciones_EjecucionId ya existe';
GO

-- Índice para evaluaciones que requieren seguimiento
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evaluaciones_RequiereSeguimiento' AND object_id = OBJECT_ID('evaluaciones'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Evaluaciones_RequiereSeguimiento
    ON evaluaciones(requiere_seguimiento, creado_en DESC)
    WHERE requiere_seguimiento = 1
    INCLUDE (orden_id, evaluador_id, score_calidad_total);
    PRINT '  ✅ Índice IX_Evaluaciones_RequiereSeguimiento creado (filtrado)';
END
ELSE
    PRINT '  ⏭️  Índice IX_Evaluaciones_RequiereSeguimiento ya existe';
GO

PRINT '';
GO

-- =====================================================
-- TABLA: evaluacion_detalles
-- =====================================================

PRINT '📊 Procesando tabla: evaluacion_detalles';
GO

-- Índice para relación con evaluaciones (FK) - CRÍTICO para Include()
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EvaluacionDetalles_EvaluacionId' AND object_id = OBJECT_ID('evaluacion_detalles'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_EvaluacionDetalles_EvaluacionId
    ON evaluacion_detalles(evaluacion_id)
    INCLUDE (fase, descripcion, score_fase, lugar, creado_en);
    PRINT '  ✅ Índice IX_EvaluacionDetalles_EvaluacionId creado (CRÍTICO para Include)';
END
ELSE
    PRINT '  ⏭️  Índice IX_EvaluacionDetalles_EvaluacionId ya existe';
GO

-- Índice para búsquedas por fase
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EvaluacionDetalles_Fase' AND object_id = OBJECT_ID('evaluacion_detalles'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_EvaluacionDetalles_Fase
    ON evaluacion_detalles(fase, evaluacion_id)
    INCLUDE (score_fase, descripcion);
    PRINT '  ✅ Índice IX_EvaluacionDetalles_Fase creado';
END
ELSE
    PRINT '  ⏭️  Índice IX_EvaluacionDetalles_Fase ya existe';
GO

PRINT '';
GO

-- =====================================================
-- TABLA: evaluacion_fotos
-- =====================================================

PRINT '📊 Procesando tabla: evaluacion_fotos';
GO

-- Índice para relación con detalles (FK) - CRÍTICO para Include()
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EvaluacionFotos_DetalleId' AND object_id = OBJECT_ID('evaluacion_fotos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_EvaluacionFotos_DetalleId
    ON evaluacion_fotos(detalle_id)
    INCLUDE (documento_id, tipo, descripcion, creado_en);
    PRINT '  ✅ Índice IX_EvaluacionFotos_DetalleId creado (CRÍTICO para Include)';
END
ELSE
    PRINT '  ⏭️  Índice IX_EvaluacionFotos_DetalleId ya existe';
GO

-- Índice para búsquedas por tipo de foto
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EvaluacionFotos_Tipo' AND object_id = OBJECT_ID('evaluacion_fotos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_EvaluacionFotos_Tipo
    ON evaluacion_fotos(tipo, detalle_id)
    WHERE tipo IS NOT NULL
    INCLUDE (documento_id, descripcion);
    PRINT '  ✅ Índice IX_EvaluacionFotos_Tipo creado (filtrado)';
END
ELSE
    PRINT '  ⏭️  Índice IX_EvaluacionFotos_Tipo ya existe';
GO

PRINT '';
PRINT '✅ ¡Todos los índices para Evaluaciones creados/verificados exitosamente!';
PRINT '';
PRINT '📊 Resumen de índices creados:';
PRINT '   - evaluaciones: 4 índices';
PRINT '   - evaluacion_detalles: 2 índices';
PRINT '   - evaluacion_fotos: 2 índices';
PRINT '   TOTAL: 8 índices';
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

PRINT '📋 Índices en tabla evaluaciones:';
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
WHERE i.object_id = OBJECT_ID('evaluaciones')
  AND i.name LIKE 'IX_%'
ORDER BY i.name;
GO

PRINT '';
PRINT '✅ Script completado exitosamente';
GO
