-- =====================================================
-- MIGRACIÓN: Refactorización Tabla Viáticos
-- Fecha: 2025-12-10
-- Descripción: 
--   1. Hacer orden_id NULLABLE (viáticos independientes)
--   2. Agregar vehiculo_id (tracking de vehículo)
--   3. Actualizar constraints
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🔄 Iniciando migración de finance_gastos_viaticos...';
PRINT '';
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
ELSE
    PRINT '  ⏭️  FK_gastos_viaticos_orden no existe';
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
ELSE
    PRINT '  ⏭️  Columna vehiculo_id ya existe';
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
ELSE
    PRINT '  ⏭️  FK_gastos_viaticos_vehiculo ya existe';
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
ELSE
    PRINT '  ⏭️  FK_gastos_viaticos_orden ya existe';
GO

PRINT '';
PRINT '✅ Migración de finance_gastos_viaticos completada';
PRINT '';
GO
