-- =====================================================================================
-- CORRECCIÓN DE ROL ADMINISTRADOR - Fix_Rol_Administrador.sql
-- =====================================================================================
--
-- ¿QUÉ HACE ESTE SCRIPT?
-- Corrige la inconsistencia de nombres de roles en la tabla auth_usuarios.
-- Cambia "ADMINISTRADOR" por "ADMINISTRACION" para que coincida con el enum RolUsuario.
--
-- ¿POR QUÉ ES NECESARIO?
-- El enum RolUsuario define ADMINISTRACION = 3, pero algunos usuarios pueden tener
-- "ADMINISTRADOR" guardado en la base de datos, causando errores de autorización.
--
-- EJECUCIÓN:
-- Ejecutar en SQL Server Management Studio o similar contra la base de datos cabs_pruebas
--
-- =====================================================================================

USE cabs_pruebas;
GO

-- Verificar usuarios con rol incorrecto antes de la corrección
PRINT 'Usuarios con rol ADMINISTRADOR antes de la corrección:';
SELECT id, nombre, apellido, email, rol
FROM auth_usuarios
WHERE rol = 'ADMINISTRADOR';

-- Corregir el rol de ADMINISTRADOR a ADMINISTRACION
UPDATE auth_usuarios
SET rol = 'ADMINISTRACION',
    actualizado_en = GETUTCDATE()
WHERE rol = 'ADMINISTRADOR';

-- Verificar que la corrección se aplicó correctamente
PRINT 'Usuarios con rol corregido:';
SELECT id, nombre, apellido, email, rol, actualizado_en
FROM auth_usuarios
WHERE rol = 'ADMINISTRACION'
ORDER BY actualizado_en DESC;

PRINT 'Corrección completada exitosamente.';
GO