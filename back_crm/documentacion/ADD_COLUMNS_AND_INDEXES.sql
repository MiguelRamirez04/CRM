-- =====================================================
-- SCRIPT SIMPLE: Agregar Columnas e Índices
-- Fecha: 2025-12-10
-- Descripción: Solo ALTER TABLE y CREATE INDEX
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🔄 Agregando columnas nuevas e índices...';
PRINT '';
GO

-- =====================================================
-- PASO 1: Agregar columna vehiculo_id a finance_gastos_viaticos
-- =====================================================

PRINT '📋 Modificando finance_gastos_viaticos...';
GO

-- Agregar columna vehiculo_id
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('finance_gastos_viaticos') AND name = 'vehiculo_id')
BEGIN
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    ADD [vehiculo_id] [int] NULL;
    PRINT '  ✅ Columna vehiculo_id agregada';
END
ELSE
    PRINT '  ⏭️  Columna vehiculo_id ya existe';
GO

-- Hacer orden_id NULLABLE (si no lo es)
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('finance_gastos_viaticos') 
    AND name = 'orden_id' 
    AND is_nullable = 0
)
BEGIN
    -- Primero eliminar FK si existe
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_gastos_viaticos_orden')
    BEGIN
        ALTER TABLE [dbo].[finance_gastos_viaticos]
        DROP CONSTRAINT [FK_gastos_viaticos_orden];
        PRINT '  ✅ FK_gastos_viaticos_orden eliminada temporalmente';
    END
    
    -- Hacer nullable
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    ALTER COLUMN [orden_id] [int] NULL;
    PRINT '  ✅ orden_id ahora es NULLABLE';
    
    -- Recrear FK
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    WITH NOCHECK ADD CONSTRAINT [FK_gastos_viaticos_orden] 
    FOREIGN KEY([orden_id])
    REFERENCES [dbo].[ops_ordenes_trabajo] ([id])
    ON DELETE CASCADE;
    
    ALTER TABLE [dbo].[finance_gastos_viaticos] 
    NOCHECK CONSTRAINT [FK_gastos_viaticos_orden];
    
    PRINT '  ✅ FK_gastos_viaticos_orden recreada';
END
ELSE
    PRINT '  ⏭️  orden_id ya es NULLABLE';
GO

-- Agregar FK a fleet_vehiculos
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_gastos_viaticos_vehiculo')
BEGIN
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    ADD CONSTRAINT [FK_gastos_viaticos_vehiculo] 
    FOREIGN KEY([vehiculo_id])
    REFERENCES [dbo].[fleet_vehiculos] ([id]);
    PRINT '  ✅ FK_gastos_viaticos_vehiculo creada';
END
ELSE
    PRINT '  ⏭️  FK_gastos_viaticos_vehiculo ya existe';
GO

-- =====================================================
-- PASO 2: Agregar columna disponible a fleet_vehiculos
-- =====================================================

PRINT '';
PRINT '📋 Modificando fleet_vehiculos...';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('fleet_vehiculos') AND name = 'disponible')
BEGIN
    ALTER TABLE [dbo].[fleet_vehiculos]
    ADD [disponible] [bit] NOT NULL DEFAULT 1;
    PRINT '  ✅ Columna disponible agregada (default: 1)';
END
ELSE
    PRINT '  ⏭️  Columna disponible ya existe';
GO

-- Actualizar vehículos en uso
UPDATE v
SET v.disponible = 0
FROM [dbo].[fleet_vehiculos] v
INNER JOIN [dbo].[fleet_uso_vehiculos] u ON v.id = u.vehiculo_id
WHERE u.estado = 'EN_USO'
  AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('fleet_vehiculos') AND name = 'disponible');

PRINT '  ✅ Vehículos en uso actualizados';
GO

-- =====================================================
-- PASO 3: Crear índices para finance_gastos_viaticos
-- =====================================================

PRINT '';
PRINT '📋 Creando índices para finance_gastos_viaticos...';
GO

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

-- =====================================================
-- PASO 4: Crear índices para fleet_vehiculos
-- =====================================================

PRINT '';
PRINT '📋 Creando índices para fleet_vehiculos...';
GO

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
PRINT '✅ ¡Migración completada exitosamente!';
PRINT '';
PRINT '📊 Resumen:';
PRINT '   - finance_gastos_viaticos:';
PRINT '     • vehiculo_id agregado';
PRINT '     • orden_id ahora nullable';
PRINT '     • 6 índices creados';
PRINT '   - fleet_vehiculos:';
PRINT '     • disponible agregado';
PRINT '     • 3 índices creados';
PRINT '';
GO
