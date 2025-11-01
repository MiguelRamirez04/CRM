-- =====================================================================================
-- MIGRACIÓN: Agregar campos de contacto a sales_cotizaciones
-- =====================================================================================
-- 
-- DESCRIPCIÓN:
-- Agrega columnas de teléfono y correo electrónico a la tabla sales_cotizaciones
-- para permitir información de contacto del cliente en las cotizaciones.
--
-- COLUMNAS A AGREGAR:
-- - telefono: BIGINT NULL (permite 10-12 dígitos, ejemplo: 6178907616)
-- - correo: VARCHAR(150) NULL (email del contacto)
--
-- VALIDACIONES:
-- - Teléfono mínimo 5 dígitos (para números cortos/extensiones)
-- - Teléfono máximo 15 dígitos (para números internacionales)
-- - Correo máximo 150 caracteres
--
-- FECHA: 2025-11-01
-- AUTOR: Sistema CRM
-- =====================================================================================

USE [cabs_pruebas]
GO

-- Verificar que la tabla existe antes de intentar modificarla
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'sales_cotizaciones')
BEGIN
    PRINT 'ERROR: La tabla sales_cotizaciones no existe en la base de datos.'
    RETURN;
END
GO

-- Verificar que las columnas NO existan antes de agregarlas
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('sales_cotizaciones') AND name = 'telefono')
BEGIN
    PRINT 'ADVERTENCIA: La columna telefono ya existe en sales_cotizaciones. Se omitirá la adición.'
END
ELSE
BEGIN
    -- Agregar columna telefono
    ALTER TABLE [dbo].[sales_cotizaciones]
    ADD [telefono] BIGINT NULL;
    
    PRINT 'Columna telefono agregada correctamente.';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('sales_cotizaciones') AND name = 'correo')
BEGIN
    PRINT 'ADVERTENCIA: La columna correo ya existe en sales_cotizaciones. Se omitirá la adición.'
END
ELSE
BEGIN
    -- Agregar columna correo
    ALTER TABLE [dbo].[sales_cotizaciones]
    ADD [correo] VARCHAR(150) NULL;
    
    PRINT 'Columna correo agregada correctamente.';
END
GO

-- Agregar constraint para validar que el teléfono tenga entre 5 y 15 dígitos
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_cotizaciones_telefono_valido')
BEGIN
    ALTER TABLE [dbo].[sales_cotizaciones]
    ADD CONSTRAINT [CK_cotizaciones_telefono_valido] 
    CHECK ([telefono] IS NULL OR ([telefono] >= 10000 AND [telefono] <= 999999999999999));
    
    PRINT 'Constraint CK_cotizaciones_telefono_valido creado correctamente.';
END
ELSE
BEGIN
    PRINT 'ADVERTENCIA: Constraint CK_cotizaciones_telefono_valido ya existe.';
END
GO

-- Agregar constraint para validar que el correo no esté vacío si se proporciona
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_cotizaciones_correo_valido')
BEGIN
    ALTER TABLE [dbo].[sales_cotizaciones]
    ADD CONSTRAINT [CK_cotizaciones_correo_valido] 
    CHECK ([correo] IS NULL OR LEN([correo]) >= 5);
    
    PRINT 'Constraint CK_cotizaciones_correo_valido creado correctamente.';
END
ELSE
BEGIN
    PRINT 'ADVERTENCIA: Constraint CK_cotizaciones_correo_valido ya existe.';
END
GO

-- Crear índice en correo para búsquedas rápidas (opcional pero recomendado)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_cotizaciones_correo' AND object_id = OBJECT_ID('sales_cotizaciones'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_cotizaciones_correo] 
    ON [dbo].[sales_cotizaciones]([correo])
    WHERE [correo] IS NOT NULL;
    
    PRINT 'Índice IX_cotizaciones_correo creado correctamente.';
END
ELSE
BEGIN
    PRINT 'ADVERTENCIA: Índice IX_cotizaciones_correo ya existe.';
END
GO

-- Verificación final
PRINT '';
PRINT '=====================================================================================';
PRINT 'MIGRACIÓN COMPLETADA';
PRINT '=====================================================================================';
PRINT 'Estructura actualizada de sales_cotizaciones:';
PRINT '';

SELECT 
    COLUMN_NAME AS 'Columna',
    DATA_TYPE AS 'Tipo',
    CHARACTER_MAXIMUM_LENGTH AS 'Longitud',
    IS_NULLABLE AS 'Nullable'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'sales_cotizaciones' AND COLUMN_NAME IN ('telefono', 'correo')
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT 'Constraints de validación:';
SELECT name AS 'Constraint', definition AS 'Definición'
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('sales_cotizaciones') 
AND name IN ('CK_cotizaciones_telefono_valido', 'CK_cotizaciones_correo_valido');

PRINT '';
PRINT '=====================================================================================';
GO
