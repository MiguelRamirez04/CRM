-- =====================================================
-- ÍNDICES ACTUALIZADOS PARA VIÁTICOS Y VEHÍCULOS
-- Fecha: 2025-12-10
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🔍 Creando índices actualizados para Viáticos y Vehículos...';
PRINT '';
GO

-- =====================================================
-- ÍNDICES PARA finance_gastos_viaticos
-- =====================================================

PRINT '📊 Procesando tabla: finance_gastos_viaticos';
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
ELSE
    PRINT '  ⏭️  Índice IX_GastosViaticos_VehiculoId ya existe';
GO

-- Índice para búsquedas por OrdenId (ahora con filtro WHERE)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_OrdenId' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_OrdenId
    ON finance_gastos_viaticos(orden_id)
    WHERE orden_id IS NOT NULL
    INCLUDE (fecha, monto_total, tiene_factura, lugar_destino);
    PRINT '  ✅ Índice IX_GastosViaticos_OrdenId creado (filtrado)';
END
ELSE
    PRINT '  ⏭️  Índice IX_GastosViaticos_OrdenId ya existe';
GO

-- Índice para búsquedas por Fecha
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_Fecha' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_Fecha
    ON finance_gastos_viaticos(fecha DESC)
    INCLUDE (orden_id, vehiculo_id, monto_total, tiene_factura);
    PRINT '  ✅ Índice IX_GastosViaticos_Fecha creado';
END
ELSE
    PRINT '  ⏭️  Índice IX_GastosViaticos_Fecha ya existe';
GO

PRINT '';
GO

-- =====================================================
-- ÍNDICES PARA fleet_vehiculos
-- =====================================================

PRINT '📊 Procesando tabla: fleet_vehiculos';
GO

-- Índice para vehículos disponibles (muy usado para selección)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculos_Disponible' AND object_id = OBJECT_ID('fleet_vehiculos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Vehiculos_Disponible
    ON fleet_vehiculos(disponible, activo)
    WHERE disponible = 1 AND activo = 1
    INCLUDE (nombre_vehiculo, placas, tipo_vehiculo, transmision);
    PRINT '  ✅ Índice IX_Vehiculos_Disponible creado (filtrado)';
END
ELSE
    PRINT '  ⏭️  Índice IX_Vehiculos_Disponible ya existe';
GO

PRINT '';
PRINT '✅ Índices actualizados creados exitosamente';
PRINT '';
PRINT '📊 Resumen:';
PRINT '   - finance_gastos_viaticos: 3 índices';
PRINT '   - fleet_vehiculos: 1 índice';
PRINT '   TOTAL: 4 índices';
PRINT '';
GO
