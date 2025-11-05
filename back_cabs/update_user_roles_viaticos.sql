-- =====================================================================================
-- Script: Actualizar rol de usuario de prueba para acceder a Viáticos
-- =====================================================================================
-- Fecha: 2025-01-04
-- Descripción: Actualiza el rol del usuario de prueba para que pueda acceder
--              al módulo de Viáticos (requiere rol ADMINISTRACION, RECEPCION o SOPORTE)
-- =====================================================================================

-- Ver usuarios actuales y sus roles
SELECT 
    id,
    nombre,
    apellido,
    correo as email,
    rol,
    activo
FROM auth_usuarios
ORDER BY id;

-- Actualizar usuario de prueba (ajustar el WHERE según tu usuario)
-- Opción 1: Si el usuario tiene email específico
UPDATE auth_usuarios 
SET 
    rol = 'ADMINISTRACION',
    actualizado_en = GETDATE()
WHERE correo LIKE '%string%' OR nombre = 'string';

-- Opción 2: Si quieres actualizar todos los usuarios activos sin rol
UPDATE auth_usuarios 
SET 
    rol = 'ADMINISTRACION',
    actualizado_en = GETDATE()
WHERE rol IS NULL OR rol = '' OR rol NOT IN ('ADMINISTRACION', 'RECEPCION', 'SOPORTE');

-- Verificar actualización
SELECT 
    id,
    nombre,
    apellido,
    correo as email,
    rol,
    activo,
    actualizado_en
FROM auth_usuarios
ORDER BY actualizado_en DESC;

PRINT '============================================';
PRINT 'Roles actualizados exitosamente';
PRINT 'Roles válidos: ADMINISTRACION, RECEPCION, SOPORTE';
PRINT '============================================';
