-- =====================================================================================
-- MIGRACIÓN: Cambiar tipo de columna telefono de INT a BIGINT
-- =====================================================================================
-- 
-- PROBLEMA: La columna telefono como INT (máx: 2,147,483,647) no puede almacenar
-- números de teléfono como 6182171064 que causan overflow.
-- 
-- SOLUCIÓN: Cambiar el tipo de datos a BIGINT para soportar números grandes
-- (máx: 9,223,372,036,854,775,807)
--
-- CUÁNDO EJECUTAR: Antes de usar la API con números de teléfono que empiecen con 6, 7, 8, o 9
--
-- =====================================================================================

USE [CRM_CABS];
GO

-- Verificar el tipo actual de la columna
SELECT 
    TABLE_NAME, 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'auth_usuarios' AND COLUMN_NAME = 'telefono';
GO

-- Cambiar el tipo de dato de INT a BIGINT
ALTER TABLE [dbo].[auth_usuarios]
ALTER COLUMN [telefono] BIGINT NOT NULL;
GO

-- Verificar que el cambio se aplicó correctamente
SELECT 
    TABLE_NAME, 
    COLUMN_NAME, 
    DATA_TYPE, 
    NUMERIC_PRECISION
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'auth_usuarios' AND COLUMN_NAME = 'telefono';
GO

-- Probar inserción de un número grande
PRINT 'Migración completada exitosamente. La columna telefono ahora es BIGINT.';
PRINT 'Ahora puedes registrar usuarios con números como 6182171064 sin problemas.';
GO
