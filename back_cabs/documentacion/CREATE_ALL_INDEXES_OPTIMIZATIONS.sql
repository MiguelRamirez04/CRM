-- =====================================================
-- SCRIPT CONSOLIDADO: Todas las Optimizaciones
-- Fecha: 2025-12-10
-- Descripción: Índices para Evaluaciones y Viáticos
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🚀 Iniciando creación de TODOS los índices de optimización...';
PRINT '';
GO

-- =====================================================
-- MÓDULO: EVALUACIONES
-- =====================================================

PRINT '📊 MÓDULO: EVALUACIONES';
PRINT '========================';
GO

-- Tabla: evaluaciones
PRINT 'Procesando: evaluaciones';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evaluaciones_OrdenId' AND object_id = OBJECT_ID('evaluaciones'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Evaluaciones_OrdenId
    ON evaluaciones(orden_id)
    INCLUDE (evaluador_id, creado_en, score_calidad_total, requiere_seguimiento);
    PRINT '  ✅ IX_Evaluaciones_OrdenId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evaluaciones_EvaluadorId_CreadoEn' AND object_id = OBJECT_ID('evaluaciones'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Evaluaciones_EvaluadorId_CreadoEn
    ON evaluaciones(evaluador_id, creado_en DESC)
    INCLUDE (orden_id, score_calidad_total, requiere_seguimiento);
    PRINT '  ✅ IX_Evaluaciones_EvaluadorId_CreadoEn';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evaluaciones_EjecucionId' AND object_id = OBJECT_ID('evaluaciones'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Evaluaciones_EjecucionId
    ON evaluaciones(ejecucion_id)
    WHERE ejecucion_id IS NOT NULL
    INCLUDE (orden_id, evaluador_id, creado_en);
    PRINT '  ✅ IX_Evaluaciones_EjecucionId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evaluaciones_RequiereSeguimiento' AND object_id = OBJECT_ID('evaluaciones'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Evaluaciones_RequiereSeguimiento
    ON evaluaciones(requiere_seguimiento, creado_en DESC)
    WHERE requiere_seguimiento = 1
    INCLUDE (orden_id, evaluador_id, score_calidad_total);
    PRINT '  ✅ IX_Evaluaciones_RequiereSeguimiento';
END
GO

-- Tabla: evaluacion_detalles
PRINT 'Procesando: evaluacion_detalles';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EvaluacionDetalles_EvaluacionId' AND object_id = OBJECT_ID('evaluacion_detalles'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_EvaluacionDetalles_EvaluacionId
    ON evaluacion_detalles(evaluacion_id)
    INCLUDE (fase, descripcion, score_fase, lugar, creado_en);
    PRINT '  ✅ IX_EvaluacionDetalles_EvaluacionId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EvaluacionDetalles_Fase' AND object_id = OBJECT_ID('evaluacion_detalles'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_EvaluacionDetalles_Fase
    ON evaluacion_detalles(fase, evaluacion_id)
    INCLUDE (score_fase, descripcion);
    PRINT '  ✅ IX_EvaluacionDetalles_Fase';
END
GO

-- Tabla: evaluacion_fotos
PRINT 'Procesando: evaluacion_fotos';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EvaluacionFotos_DetalleId' AND object_id = OBJECT_ID('evaluacion_fotos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_EvaluacionFotos_DetalleId
    ON evaluacion_fotos(detalle_id)
    INCLUDE (documento_id, tipo, descripcion, creado_en);
    PRINT '  ✅ IX_EvaluacionFotos_DetalleId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EvaluacionFotos_Tipo' AND object_id = OBJECT_ID('evaluacion_fotos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_EvaluacionFotos_Tipo
    ON evaluacion_fotos(tipo, detalle_id)
    WHERE tipo IS NOT NULL
    INCLUDE (documento_id, descripcion);
    PRINT '  ✅ IX_EvaluacionFotos_Tipo';
END
GO

PRINT '';
GO

-- =====================================================
-- MÓDULO: VIÁTICOS
-- =====================================================

PRINT '📊 MÓDULO: VIÁTICOS';
PRINT '===================';
GO

-- Tabla: finance_gastos_viaticos
PRINT 'Procesando: finance_gastos_viaticos';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_VehiculoId' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_VehiculoId
    ON finance_gastos_viaticos(vehiculo_id)
    WHERE vehiculo_id IS NOT NULL
    INCLUDE (fecha, monto_total, lugar_destino, km_recorridos);
    PRINT '  ✅ IX_GastosViaticos_VehiculoId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_OrdenId' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_OrdenId
    ON finance_gastos_viaticos(orden_id)
    WHERE orden_id IS NOT NULL
    INCLUDE (fecha, monto_total, tiene_factura, lugar_destino);
    PRINT '  ✅ IX_GastosViaticos_OrdenId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_Fecha' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_Fecha
    ON finance_gastos_viaticos(fecha DESC)
    INCLUDE (orden_id, vehiculo_id, monto_total, tiene_factura);
    PRINT '  ✅ IX_GastosViaticos_Fecha';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_FechaRango' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_FechaRango
    ON finance_gastos_viaticos(fecha, orden_id)
    INCLUDE (monto_total, lugar_destino, tiene_factura, km_recorridos);
    PRINT '  ✅ IX_GastosViaticos_FechaRango';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_TieneFactura' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_TieneFactura
    ON finance_gastos_viaticos(tiene_factura, fecha DESC)
    INCLUDE (orden_id, monto_total, proveedor_nombre, descripcion);
    PRINT '  ✅ IX_GastosViaticos_TieneFactura';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_LugarDestino' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_LugarDestino
    ON finance_gastos_viaticos(lugar_destino, fecha DESC)
    WHERE lugar_destino IS NOT NULL
    INCLUDE (monto_total, km_recorridos, orden_id);
    PRINT '  ✅ IX_GastosViaticos_LugarDestino';
END
GO

-- Tabla: fleet_vehiculos
PRINT 'Procesando: fleet_vehiculos';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculos_Disponible' AND object_id = OBJECT_ID('fleet_vehiculos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Vehiculos_Disponible
    ON fleet_vehiculos(disponible, activo)
    WHERE disponible = 1 AND activo = 1
    INCLUDE (nombre_vehiculo, placas, tipo_vehiculo, transmision);
    PRINT '  ✅ IX_Vehiculos_Disponible';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculos_Placas' AND object_id = OBJECT_ID('fleet_vehiculos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Vehiculos_Placas
    ON fleet_vehiculos(placas)
    WHERE placas IS NOT NULL
    INCLUDE (nombre_vehiculo, activo, disponible);
    PRINT '  ✅ IX_Vehiculos_Placas';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculos_Activo' AND object_id = OBJECT_ID('fleet_vehiculos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Vehiculos_Activo
    ON fleet_vehiculos(activo, disponible)
    INCLUDE (nombre_vehiculo, placas, tipo_vehiculo);
    PRINT '  ✅ IX_Vehiculos_Activo';
END
GO

PRINT '';
PRINT '✅ ¡TODOS LOS ÍNDICES CREADOS EXITOSAMENTE!';
PRINT '';
PRINT '📊 RESUMEN FINAL:';
PRINT '   EVALUACIONES:';
PRINT '     - evaluaciones: 4 índices';
PRINT '     - evaluacion_detalles: 2 índices';
PRINT '     - evaluacion_fotos: 2 índices';
PRINT '   VIÁTICOS:';
PRINT '     - finance_gastos_viaticos: 6 índices';
PRINT '     - fleet_vehiculos: 3 índices';
PRINT '';
PRINT '   TOTAL: 17 índices';
PRINT '';
PRINT '💡 Recomendaciones:';
PRINT '   1. Ejecuta UPDATE STATISTICS para actualizar estadísticas';
PRINT '   2. Monitorea uso con sys.dm_db_index_usage_stats';
PRINT '   3. Reorganiza/reconstruye índices mensualmente';
PRINT '';
GO
