-- =====================================================
-- SCRIPT ALTERNATIVO: MIGRACIÓN SIN ELIMINAR DATOS
-- Fecha: 2025-12-10
-- Descripción: Modifica las tablas existentes sin perder datos
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🔄 Iniciando migración de tablas existentes...';
PRINT '✅ Este script NO eliminará datos';
PRINT '';
GO

-- =====================================================
-- OPCIÓN 1: MODIFICAR TABLAS EXISTENTES (RECOMENDADO)
-- =====================================================

-- Este es el enfoque recomendado si ya tienes datos
-- Ejecuta los scripts de migración en orden:

PRINT '📝 EJECUTA ESTOS SCRIPTS EN ORDEN:';
PRINT '';
PRINT '1. MIGRATION_001_Viaticos_RefactorTable.sql';
PRINT '   - Hace orden_id NULLABLE';
PRINT '   - Agrega vehiculo_id';
PRINT '   - Crea FKs';
PRINT '';
PRINT '2. MIGRATION_002_Vehiculos_AddDisponible.sql';
PRINT '   - Agrega campo disponible a fleet_vehiculos';
PRINT '   - Actualiza vehículos en uso';
PRINT '';
PRINT '3. MIGRATION_003_Indexes_Updated.sql';
PRINT '   - Crea todos los índices';
PRINT '';
PRINT '✅ Tus datos se mantendrán intactos';
PRINT '';
GO

-- =====================================================
-- VERIFICACIÓN: ¿Necesitas eliminar las tablas?
-- =====================================================

PRINT '⚠️  ADVERTENCIA:';
PRINT '   Solo usa DROP_TABLES_VIATICOS.sql si:';
PRINT '   - NO tienes datos importantes';
PRINT '   - Quieres empezar desde cero';
PRINT '   - Ya hiciste un BACKUP completo';
PRINT '';
GO
