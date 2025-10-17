-- =====================================================================================
-- CORRECCIÓN DE ROLES DE USUARIOS
-- =====================================================================================
-- Este script actualiza los roles de usuarios para que coincidan con el enum RolUsuario
-- Roles válidos: RECEPCION, SOPORTE, ADMINISTRACION
-- =====================================================================================

USE cabs_pruebas;
GO

-- Ver los roles actuales
PRINT '=== ROLES ACTUALES ===';
SELECT id, nombre, apellido, correo, rol 
FROM auth_usuarios
ORDER BY id;

-- Actualizar roles incorrectos a los valores correctos del enum
PRINT '';
PRINT '=== ACTUALIZANDO ROLES ===';

-- Actualizar variaciones de "Admin" a "ADMINISTRACION"
UPDATE auth_usuarios 
SET rol = 'ADMINISTRACION'
WHERE rol IN ('Admin', 'ADMIN', 'Administrador', 'ADMINISTRADOR', 'admin', 'administrador');

PRINT 'Usuarios Admin actualizados: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- Actualizar variaciones de "Soporte" a "SOPORTE"
UPDATE auth_usuarios 
SET rol = 'SOPORTE'
WHERE rol IN ('Soporte', 'soporte', 'TECNICO', 'Tecnico', 'tecnico', 'Support');

PRINT 'Usuarios Soporte actualizados: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- Actualizar variaciones de "Recepcion" a "RECEPCION"
UPDATE auth_usuarios 
SET rol = 'RECEPCION'
WHERE rol IN ('Recepcion', 'recepcion', 'Recepción', 'RECEPCIÓN', 'Reception');

PRINT 'Usuarios Recepción actualizados: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- Ver los roles después de la actualización
PRINT '';
PRINT '=== ROLES DESPUÉS DE LA ACTUALIZACIÓN ===';
SELECT id, nombre, apellido, correo, rol 
FROM auth_usuarios
ORDER BY id;

-- Verificar si hay roles inválidos
PRINT '';
PRINT '=== VERIFICACIÓN DE ROLES INVÁLIDOS ===';
SELECT id, nombre, apellido, correo, rol 
FROM auth_usuarios
WHERE rol NOT IN ('RECEPCION', 'SOPORTE', 'ADMINISTRACION');

IF @@ROWCOUNT > 0
BEGIN
    PRINT '⚠️ ADVERTENCIA: Existen usuarios con roles inválidos';
END
ELSE
BEGIN
    PRINT '✅ Todos los roles son válidos';
END

GO
