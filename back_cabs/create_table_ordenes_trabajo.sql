-- Script para crear la tabla ops_ordenes_trabajo si no existe
USE cabs_pruebas;
GO

-- Verificar si la tabla existe
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ops_ordenes_trabajo]') AND type in (N'U'))
BEGIN
    PRINT 'Creando tabla ops_ordenes_trabajo...'
    
    CREATE TABLE [dbo].[ops_ordenes_trabajo] (
        [id] INT IDENTITY(1,1) NOT NULL,
        [nuevo_cliente] BIT NULL,
        [nombre_cliente] NVARCHAR(120) NULL,
        [cliente_id] INT NULL, -- PERMITIR NULL para clientes nuevos
        [creado_por_user_id] INT NOT NULL,
        [asignada_a_user_id] INT NULL,
        [notas] NVARCHAR(MAX) NULL,
        [cita_programada_inicio] DATETIME2(0) NULL,
        [cita_programada_fin] DATETIME2(0) NULL,
        [modalidad] NVARCHAR(50) NULL,
        [tipo_orden] NVARCHAR(50) NULL,
        [prioridad] INT NULL,
        [estado] NVARCHAR(20) NOT NULL DEFAULT 'CAPTURADA',
        [ubicacion_text] NVARCHAR(MAX) NULL,
        [requiere_factura] BIT NOT NULL DEFAULT 0,
        [estado_facturado] NVARCHAR(50) NULL,
        [factura_folio] NVARCHAR(50) NULL,
        [creado_en] DATETIME2(0) NOT NULL DEFAULT GETDATE(),
        [actualizado_en] DATETIME2(0) NULL,
        [costo_real] DECIMAL(12,2) NULL,
        [costo_estimado] DECIMAL(12,2) NULL,
        
        CONSTRAINT [PK_ops_ordenes_trabajo] PRIMARY KEY CLUSTERED ([id] ASC)
    );
    
    PRINT 'Tabla ops_ordenes_trabajo creada exitosamente.'
END
ELSE
BEGIN
    PRINT 'La tabla ops_ordenes_trabajo ya existe.'
    
    -- Verificar si cliente_id permite NULL
    SELECT 
        c.COLUMN_NAME,
        c.IS_NULLABLE,
        c.DATA_TYPE,
        c.CHARACTER_MAXIMUM_LENGTH
    FROM INFORMATION_SCHEMA.COLUMNS c
    WHERE c.TABLE_NAME = 'ops_ordenes_trabajo' 
        AND c.COLUMN_NAME = 'cliente_id';
    
    -- Si cliente_id no permite NULL, alterarla
    IF EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'ops_ordenes_trabajo' 
            AND COLUMN_NAME = 'cliente_id' 
            AND IS_NULLABLE = 'NO'
    )
    BEGIN
        PRINT 'Alterando columna cliente_id para permitir NULL...'
        ALTER TABLE [dbo].[ops_ordenes_trabajo] 
        ALTER COLUMN [cliente_id] INT NULL;
        PRINT 'Columna cliente_id alterada exitosamente.'
    END
    ELSE
    BEGIN
        PRINT 'La columna cliente_id ya permite NULL.'
    END
END

-- Verificar estructura final
PRINT 'Estructura actual de la tabla:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ops_ordenes_trabajo'
ORDER BY ORDINAL_POSITION;

PRINT 'Script completado.'