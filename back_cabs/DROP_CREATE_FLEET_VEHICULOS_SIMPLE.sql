    -- ============================================
    -- DROP and CREATE: fleet_vehiculos (unificada, SIN triggers - auditoría en C#)
    -- WARNING: This will DROP existing tables `fleet_vehiculos_auditoria` and `fleet_vehiculos` and all their data.
    -- Execute this on the correct database. You will lose existing data.
    -- ============================================

    USE cabs_pruebas;
    GO

    PRINT '--- Iniciando: eliminar tablas antiguas y crear nueva tabla unificada ---';

    -- Eliminar triggers relacionados (si existen)
    IF OBJECT_ID('dbo.TR_fleet_vehiculos_auditoria_INSERT','TR') IS NOT NULL
        DROP TRIGGER dbo.TR_fleet_vehiculos_auditoria_INSERT;
    IF OBJECT_ID('dbo.TR_fleet_vehiculos_auditoria_UPDATE','TR') IS NOT NULL
        DROP TRIGGER dbo.TR_fleet_vehiculos_auditoria_UPDATE;
    IF OBJECT_ID('dbo.TR_fleet_vehiculos_auditoria_auto','TR') IS NOT NULL
        DROP TRIGGER dbo.TR_fleet_vehiculos_auditoria_auto;
    GO

    -- Eliminar cualquier foreign key que apunte a las tablas antiguas (robusto)
    DECLARE @sql NVARCHAR(MAX);
    -- FKs que referencian fleet_vehiculos_auditoria
    SELECT @sql = STRING_AGG('ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name) + ';', CHAR(13))
    FROM sys.foreign_keys
    WHERE referenced_object_id = OBJECT_ID('dbo.fleet_vehiculos_auditoria');
    IF @sql IS NOT NULL
    BEGIN
        PRINT '-- Eliminando FKs que referencian fleet_vehiculos_auditoria --';
        EXEC sp_executesql @sql;
    END

    -- FKs que referencian fleet_vehiculos
    SELECT @sql = STRING_AGG('ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name) + ';', CHAR(13))
    FROM sys.foreign_keys
    WHERE referenced_object_id = OBJECT_ID('dbo.fleet_vehiculos');
    IF @sql IS NOT NULL
    BEGIN
        PRINT '-- Eliminando FKs que referencian fleet_vehiculos --';
        EXEC sp_executesql @sql;
    END
    GO

    -- Drop the old tables (if exist)
    DROP TABLE IF EXISTS dbo.fleet_vehiculos_auditoria;
    DROP TABLE IF EXISTS dbo.fleet_vehiculos;
    GO

    PRINT '-- Tablas antiguas eliminadas (si existían)';

    -- Crear la nueva tabla unificada
    CREATE TABLE dbo.fleet_vehiculos (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        tipo_vehiculo VARCHAR(50) NULL,
        transmision VARCHAR(20) NULL,
        es_de_empresa BIT NOT NULL DEFAULT(1),
        placas VARCHAR(20) NULL,
        activo BIT NOT NULL DEFAULT(1),
        nombre_vehiculo VARCHAR(100) NOT NULL DEFAULT(''),
        kilometraje INT NOT NULL, -- OBLIGATORIO
        observaciones NVARCHAR(MAX) NULL,

        -- Auditoría/Metadatos
        creado_en DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
        creado_por_usuario_id INT NULL,

        actualizado_en DATETIME2(7) NULL,
        actualizado_por_usuario_id INT NULL,

        -- Historial de cambios (JSON array)
        historial_cambios NVARCHAR(MAX) NULL,

        CONSTRAINT UQ_fleet_vehiculos_placas UNIQUE (placas),
        CONSTRAINT CK_fleet_vehiculos_kilometraje CHECK (kilometraje >= 0)
    );
    GO

    -- Índices recomendados
    CREATE NONCLUSTERED INDEX IX_fleet_vehiculos_activo ON dbo.fleet_vehiculos(activo) WHERE activo = 1;
    CREATE NONCLUSTERED INDEX IX_fleet_vehiculos_placas ON dbo.fleet_vehiculos(placas);
    CREATE NONCLUSTERED INDEX IX_fleet_vehiculos_actualizado ON dbo.fleet_vehiculos(actualizado_en) WHERE actualizado_en IS NOT NULL;
    GO

    PRINT '-- Tabla nueva `fleet_vehiculos` creada';

    PRINT '--- Script completado: nueva tabla unificada creada (sin triggers) ---';
    PRINT 'Nota: La auditoría se manejará desde el código C# usando VehiculosService.RegistrarAuditoriaAsync.';
    PRINT 'El campo historial_cambios almacenará el historial JSON de cambios en kilometraje y observaciones.';
    GO
