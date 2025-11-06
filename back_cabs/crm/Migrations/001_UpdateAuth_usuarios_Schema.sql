-- =====================================================================================
-- MIGRACIÓN: Actualizar estructura de tabla Auth_usuarios
-- =====================================================================================
-- Fecha: 2025-10-01
-- Descripción: Actualiza la tabla Auth_usuarios para coincidir con el nuevo esquema
--              - Cambia Id de uniqueidentifier a INT IDENTITY
--              - Separa nombreCompleto en nombre y apellido
--              - Agrega campo telefono
--              - Cambia password y contraseña_hash a VARCHAR(255)
--              - Cambia licencia_conducir y transmicion_habilitada a VARCHAR(50)
--              - Cambia rol a INT nullable
-- =====================================================================================

-- PASO 1: Respaldar datos existentes (si los hay)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Auth_usuarios')
BEGIN
    -- Crear tabla temporal de respaldo
    SELECT * INTO Auth_usuarios_backup FROM Auth_usuarios;
    PRINT 'Datos respaldados en Auth_usuarios_backup';
END

-- PASO 2: Eliminar tabla existente si existe
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Auth_usuarios')
BEGIN
    DROP TABLE Auth_usuarios;
    PRINT 'Tabla Auth_usuarios eliminada';
END

-- PASO 3: Crear nueva tabla con estructura actualizada
CREATE TABLE Auth_usuarios (
    id INT IDENTITY(1,1) PRIMARY KEY,                -- ID autoincremental
    creado_en DATETIME DEFAULT GETDATE() NOT NULL,   -- Fecha de creación
    actualizado_en DATETIME,                         -- Fecha y hora de actualización
    email VARCHAR(150) NOT NULL UNIQUE,              -- Correo electrónico
    password VARCHAR(255) NOT NULL,                  -- Contraseña en texto plano (temporal)
    nombre VARCHAR(100) NOT NULL,                    -- Nombre
    apellido VARCHAR(100) NOT NULL,                  -- Apellido
    telefono INT,                                    -- Teléfono (10 dígitos)
    contraseña_hash VARCHAR(255),                    -- Contraseña en hash (para migración futura)
    rol INT,                                         -- Rol del trabajador (valor numérico)
    activo BIT DEFAULT 1 NOT NULL,                   -- Activo (1 = sí, 0 = no)
    licencia_conducir VARCHAR(50),                   -- Número de licencia de conducir
    transmision_habilitada VARCHAR(50)               -- Transmisión habilitada
);

PRINT 'Tabla Auth_usuarios creada exitosamente';

-- PASO 4: Crear índices para optimizar consultas
CREATE INDEX IX_Auth_usuarios_email ON Auth_usuarios(email);
CREATE INDEX IX_Auth_usuarios_activo ON Auth_usuarios(activo);
CREATE INDEX IX_Auth_usuarios_rol ON Auth_usuarios(rol);

PRINT 'Índices creados exitosamente';

-- PASO 5: Insertar datos de respaldo si existían (ajustar según sea necesario)
-- NOTA: Este paso requiere mapeo manual si los datos antiguos tenían estructura diferente
-- Ejemplo de migración de datos:
/*
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Auth_usuarios_backup')
BEGIN
    INSERT INTO Auth_usuarios (nombre, apellido, email, password, activo, creado_en)
    SELECT 
        SUBSTRING(NombreCompleto, 1, CHARINDEX(' ', NombreCompleto + ' ') - 1) as nombre,
        SUBSTRING(NombreCompleto, CHARINDEX(' ', NombreCompleto + ' ') + 1, LEN(NombreCompleto)) as apellido,
        Email,
        ContrasenaHash, -- O Password si existía
        Activo,
        CreadoEn
    FROM Auth_usuarios_backup;
    
    PRINT 'Datos migrados desde backup';
END
*/

-- PASO 6: Verificar estructura final
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Auth_usuarios'
ORDER BY ORDINAL_POSITION;

PRINT '============================================';
PRINT 'Migración completada exitosamente';
PRINT 'IMPORTANTE: Verificar que los datos se migraron correctamente';
PRINT 'IMPORTANTE: Eliminar tabla de backup cuando esté seguro: DROP TABLE Auth_usuarios_backup';
PRINT '============================================';
