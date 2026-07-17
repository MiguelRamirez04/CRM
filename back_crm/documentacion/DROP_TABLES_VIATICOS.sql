-- =====================================================
-- SCRIPT: DROP Y RECREAR TABLAS VIÁTICOS
-- Fecha: 2025-12-10
-- ADVERTENCIA: Este script eliminará las tablas existentes
-- Asegúrate de hacer BACKUP antes de ejecutar
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '⚠️  ADVERTENCIA: Este script eliminará tablas existentes';
PRINT '⚠️  Asegúrate de tener un BACKUP de la base de datos';
PRINT '';
PRINT 'Presiona Ctrl+C para cancelar o espera 5 segundos...';
WAITFOR DELAY '00:00:05';
GO

PRINT '';
PRINT '🔄 Iniciando proceso de eliminación y recreación...';
PRINT '';
GO

-- =====================================================
-- PASO 1: BACKUP DE DATOS (OPCIONAL - DESCOMENTAR SI NECESITAS)
-- =====================================================

/*
-- Crear tablas temporales para backup
SELECT * INTO #backup_gastos_viaticos FROM finance_gastos_viaticos;
SELECT * INTO #backup_vehiculos FROM fleet_vehiculos;
PRINT '✅ Backup de datos creado en tablas temporales';
*/

-- =====================================================
-- PASO 2: ELIMINAR FOREIGN KEYS
-- =====================================================

PRINT '📋 Eliminando Foreign Keys...';
GO

-- FK de finance_gastos_viaticos a ops_ordenes_trabajo
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_gastos_viaticos_orden')
BEGIN
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    DROP CONSTRAINT [FK_gastos_viaticos_orden];
    PRINT '  ✅ FK_gastos_viaticos_orden eliminada';
END
GO

-- FK de finance_gastos_viaticos a fleet_vehiculos
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_gastos_viaticos_vehiculo')
BEGIN
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    DROP CONSTRAINT [FK_gastos_viaticos_vehiculo];
    PRINT '  ✅ FK_gastos_viaticos_vehiculo eliminada';
END
GO

-- =====================================================
-- PASO 3: ELIMINAR ÍNDICES
-- =====================================================

PRINT '';
PRINT '📋 Eliminando índices de finance_gastos_viaticos...';
GO

DECLARE @sql NVARCHAR(MAX) = '';

-- Eliminar todos los índices no clustered de finance_gastos_viaticos
SELECT @sql = @sql + 'DROP INDEX ' + i.name + ' ON finance_gastos_viaticos; '
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('finance_gastos_viaticos')
  AND i.type_desc = 'NONCLUSTERED'
  AND i.is_primary_key = 0
  AND i.is_unique_constraint = 0;

IF @sql <> ''
BEGIN
    EXEC sp_executesql @sql;
    PRINT '  ✅ Índices de finance_gastos_viaticos eliminados';
END
GO

PRINT '';
PRINT '📋 Eliminando índices de fleet_vehiculos...';
GO

DECLARE @sql2 NVARCHAR(MAX) = '';

-- Eliminar todos los índices no clustered de fleet_vehiculos
SELECT @sql2 = @sql2 + 'DROP INDEX ' + i.name + ' ON fleet_vehiculos; '
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('fleet_vehiculos')
  AND i.type_desc = 'NONCLUSTERED'
  AND i.is_primary_key = 0
  AND i.is_unique_constraint = 0
  AND i.name NOT LIKE 'PK_%';  -- No eliminar PK

IF @sql2 <> ''
BEGIN
    EXEC sp_executesql @sql2;
    PRINT '  ✅ Índices de fleet_vehiculos eliminados';
END
GO

-- =====================================================
-- PASO 4: ELIMINAR TABLAS
-- =====================================================

PRINT '';
PRINT '📋 Eliminando tablas...';
GO

-- Eliminar finance_gastos_viaticos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[finance_gastos_viaticos]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[finance_gastos_viaticos];
    PRINT '  ✅ Tabla finance_gastos_viaticos eliminada';
END
GO

-- Eliminar fleet_vehiculos (CUIDADO: esto eliminará todos los vehículos)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fleet_vehiculos]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[fleet_vehiculos];
    PRINT '  ✅ Tabla fleet_vehiculos eliminada';
END
GO

PRINT '';
PRINT '✅ Tablas eliminadas exitosamente';
PRINT '';
PRINT '📝 SIGUIENTE PASO:';
PRINT '   Ejecuta el script CREATE_TABLES_VIATICOS_COMPLETE.sql';
PRINT '   para recrear las tablas con la nueva estructura';
PRINT '';
GO
