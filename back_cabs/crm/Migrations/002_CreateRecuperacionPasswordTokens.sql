-- =====================================================================================
-- MIGRACIÓN: Crear tabla RecuperacionPasswordTokens
-- =====================================================================================
-- Fecha: 2025-11-13
-- Descripción: Crea la tabla para almacenar tokens de recuperación de contraseña
--              - Almacena email, token, expiración y estado de uso
-- =====================================================================================

-- PASO 1: Crear tabla RecuperacionPasswordTokens
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RecuperacionPasswordTokens')
BEGIN
    CREATE TABLE RecuperacionPasswordTokens (
        id INT IDENTITY(1,1) PRIMARY KEY,                -- ID autoincremental
        email VARCHAR(150) NOT NULL,                     -- Correo electrónico del usuario
        token VARCHAR(10) NOT NULL,                      -- Token de 6 dígitos
        expiracion DATETIME NOT NULL,                    -- Fecha y hora de expiración
        usado BIT DEFAULT 0 NOT NULL                     -- Si el token ya fue usado (0 = no, 1 = sí)
    );

    PRINT 'Tabla RecuperacionPasswordTokens creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla RecuperacionPasswordTokens ya existe';
END

-- PASO 2: Crear índices para optimizar consultas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('RecuperacionPasswordTokens') AND name = 'IX_RecuperacionPasswordTokens_email_token')
BEGIN
    CREATE INDEX IX_RecuperacionPasswordTokens_email_token ON RecuperacionPasswordTokens(email, token);
    PRINT 'Índice IX_RecuperacionPasswordTokens_email_token creado';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('RecuperacionPasswordTokens') AND name = 'IX_RecuperacionPasswordTokens_expiracion')
BEGIN
    CREATE INDEX IX_RecuperacionPasswordTokens_expiracion ON RecuperacionPasswordTokens(expiracion);
    PRINT 'Índice IX_RecuperacionPasswordTokens_expiracion creado';
END

PRINT 'Migración completada exitosamente';