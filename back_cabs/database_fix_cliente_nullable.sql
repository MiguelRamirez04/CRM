-- =====================================================================================
-- SCRIPT DE CORRECCIÓN: Permitir NULL en cliente_id para modelo híbrido de clientes
-- =====================================================================================
--
-- PROBLEMA: La columna cliente_id no permite NULL, pero necesitamos soportar clientes nuevos
-- donde cliente_id debe ser NULL y nombre_cliente debe tener valor.
--
-- SOLUCIÓN: Alterar la tabla para permitir NULL en cliente_id
--
-- EJECUTAR EN: Base de datos cabs_pruebas
-- =====================================================================================

USE cabs_pruebas;
GO

-- Verificar estructura actual de la tabla
PRINT 'Estructura actual de ops_ordenes_trabajo:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ops_ordenes_trabajo' 
    AND COLUMN_NAME IN ('cliente_id', 'nuevo_cliente', 'nombre_cliente');
GO

-- Alterar la columna cliente_id para permitir NULL
PRINT 'Alterando columna cliente_id para permitir NULL...';
ALTER TABLE dbo.ops_ordenes_trabajo 
ALTER COLUMN cliente_id INT NULL;
GO

-- Verificar que el cambio se aplicó correctamente
PRINT 'Estructura después del cambio:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ops_ordenes_trabajo' 
    AND COLUMN_NAME IN ('cliente_id', 'nuevo_cliente', 'nombre_cliente');
GO

-- Opcional: Agregar restricciones de validación para el modelo híbrido
PRINT 'Agregando restricción para validar modelo híbrido de clientes...';

-- Eliminar restricción existente si existe
IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_ops_ordenes_trabajo_cliente_hibrido')
BEGIN
    ALTER TABLE dbo.ops_ordenes_trabajo DROP CONSTRAINT CK_ops_ordenes_trabajo_cliente_hibrido;
    PRINT 'Restricción anterior eliminada.';
END;

-- Agregar nueva restricción que valida el modelo híbrido:
-- - Si nuevo_cliente = 1 (o NULL), entonces cliente_id debe ser NULL y nombre_cliente no debe ser NULL
-- - Si nuevo_cliente = 0, entonces cliente_id no debe ser NULL
ALTER TABLE dbo.ops_ordenes_trabajo 
ADD CONSTRAINT CK_ops_ordenes_trabajo_cliente_hibrido 
CHECK (
    (nuevo_cliente = 1 AND cliente_id IS NULL AND nombre_cliente IS NOT NULL) OR
    (nuevo_cliente = 0 AND cliente_id IS NOT NULL) OR
    (nuevo_cliente IS NULL AND cliente_id IS NULL AND nombre_cliente IS NOT NULL)
);
GO

PRINT 'Corrección completada exitosamente.';
PRINT 'Ahora la tabla soporta el modelo híbrido de clientes:';
PRINT '- Cliente nuevo: nuevo_cliente = 1, cliente_id = NULL, nombre_cliente = "Nombre"';
PRINT '- Cliente legacy: nuevo_cliente = 0, cliente_id = ID, nombre_cliente puede ser NULL';