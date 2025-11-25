-- =====================================================================================
-- MIGRACIÓN: Agregar campo cliente_telefono a tabla ordenes_trabajo
-- =====================================================================================
-- 
-- PROPOSITO: Almacenar el teléfono de clientes nuevos directamente en la orden
-- para facilitar contacto sin necesidad de crear un registro completo de cliente.
--
-- FECHA: 2025-10-16
-- AUTOR: Sistema CRM CABS
-- =====================================================================================

USE [CRM_CABS];
GO

-- Verificar que la tabla existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ordenes_trabajo' AND schema_id = SCHEMA_ID('ops'))
BEGIN
    PRINT 'ERROR: La tabla ops.ordenes_trabajo no existe. Ejecuta primero create_table_ordenes_trabajo.sql';
    RETURN;
END
GO

-- Verificar si la columna ya existe
IF EXISTS (SELECT * FROM sys.columns 
           WHERE object_id = OBJECT_ID('ops.ordenes_trabajo') 
           AND name = 'cliente_telefono')
BEGIN
    PRINT 'ADVERTENCIA: La columna cliente_telefono ya existe. No se realizarán cambios.';
    RETURN;
END
GO

PRINT 'Iniciando migración: Agregando columna cliente_telefono...';
GO

-- Agregar la columna cliente_telefono (BIGINT NULL - opcional)
ALTER TABLE [ops].[ordenes_trabajo]
ADD [cliente_telefono] BIGINT NULL;
GO

PRINT 'Columna cliente_telefono agregada exitosamente.';
GO

-- Agregar comentario descriptivo (Extended Property)
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Teléfono del cliente nuevo (solo para clientes no legacy). Formato: 10 dígitos sin espacios ni guiones. Ejemplo: 6182171064', 
    @level0type = N'SCHEMA', @level0name = 'ops',
    @level1type = N'TABLE',  @level1name = 'ordenes_trabajo',
    @level2type = N'COLUMN', @level2name = 'cliente_telefono';
GO

-- Verificar la estructura final
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'ops' 
  AND TABLE_NAME = 'ordenes_trabajo'
  AND COLUMN_NAME IN ('nombre_cliente', 'cliente_id', 'cliente_telefono', 'nuevo_cliente')
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT '✅ Migración completada exitosamente!';
PRINT '';
PRINT 'Ahora puedes:';
PRINT '1. Crear órdenes con cliente nuevo incluyendo teléfono';
PRINT '2. Usar GET /api/Recepcion/clientes/nuevos para listar clientes nuevos';
PRINT '';
GO
