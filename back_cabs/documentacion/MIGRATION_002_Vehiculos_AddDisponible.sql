-- =====================================================
-- MIGRACIÓN: Agregar campo disponible a fleet_vehiculos
-- Fecha: 2025-12-10
-- Descripción: Control de disponibilidad de vehículos
-- =====================================================

USE CABS_Pruebas;
GO

PRINT '🔄 Iniciando migración de fleet_vehiculos...';
PRINT '';
GO

-- =====================================================
-- PASO 1: Agregar columna disponible
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('fleet_vehiculos') AND name = 'disponible')
BEGIN
    ALTER TABLE [dbo].[fleet_vehiculos]
    ADD [disponible] [bit] NOT NULL DEFAULT 1;
    PRINT '  ✅ Columna disponible agregada (default: 1 = disponible)';
END
ELSE
    PRINT '  ⏭️  Columna disponible ya existe';
GO

-- =====================================================
-- PASO 2: Actualizar vehículos existentes
-- =====================================================

-- Marcar como NO disponibles los vehículos actualmente en uso
UPDATE v
SET v.disponible = 0
FROM [dbo].[fleet_vehiculos] v
INNER JOIN [dbo].[fleet_uso_vehiculos] u ON v.id = u.vehiculo_id
WHERE u.estado = 'EN_USO';

PRINT '  ✅ Vehículos en uso marcados como no disponibles';
GO

PRINT '';
PRINT '✅ Migración de fleet_vehiculos completada';
PRINT '';
GO
