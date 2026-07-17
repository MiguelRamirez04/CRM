-- ============================================
-- ROLLBACK DE BACKUP - RESTAURACIÓN
-- Fecha: 2025-10-18
-- ADVERTENCIA: Solo ejecutar si necesitas revertir cambios
-- ============================================

USE [tu_base_de_datos]; -- Cambiar por el nombre real
GO

SET NOCOUNT ON;
GO

PRINT '╔═══════════════════════════════════════════════════════════╗';
PRINT '║  ⚠️  SCRIPT DE ROLLBACK - RESTAURACIÓN DE BACKUP  ⚠️     ║';
PRINT '║  Fecha: 2025-10-18                                        ║';
PRINT '╚═══════════════════════════════════════════════════════════╝';
PRINT '';
PRINT '⚠️  ⚠️  ⚠️   ADVERTENCIA CRÍTICA   ⚠️  ⚠️  ⚠️ ';
PRINT '';
PRINT 'Este script ELIMINARÁ todos los datos actuales y los';
PRINT 'reemplazará con el backup del 2025-10-18.';
PRINT '';
PRINT 'DATOS QUE SERÁN REEMPLAZADOS:';
PRINT '  - files_documentos (todos los registros)';
PRINT '  - evaluacion_fotos (todos los registros)';
PRINT '  - reparacion_fotos (todos los registros)';
PRINT '';
PRINT '════════════════════════════════════════════════════════════';
PRINT '';

-- Verificar que el backup existe
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'backup_20251018')
BEGIN
    PRINT '❌ ERROR: El esquema backup_20251018 no existe.';
    PRINT '   Ejecuta primero el script backup_tablas.sql';
    PRINT '';
    RAISERROR('Backup no encontrado', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'files_documentos' AND schema_id = SCHEMA_ID('backup_20251018'))
BEGIN
    PRINT '❌ ERROR: La tabla backup_20251018.files_documentos no existe.';
    PRINT '';
    RAISERROR('Tabla de backup no encontrada', 16, 1);
    RETURN;
END

PRINT '✅ Backup verificado: backup_20251018 existe';
PRINT '';

-- ============================================
-- MOSTRAR PREVIEW DE LO QUE SE VA A HACER
-- ============================================
PRINT '📊 PREVIEW DE CAMBIOS:';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

DECLARE @current_docs INT, @backup_docs INT;
DECLARE @current_eval INT, @backup_eval INT;
DECLARE @current_rep INT, @backup_rep INT;

SELECT @current_docs = COUNT(*) FROM files_documentos;
SELECT @backup_docs = COUNT(*) FROM backup_20251018.files_documentos;

SELECT @current_eval = COUNT(*) FROM evaluacion_fotos;
SELECT @backup_eval = COUNT(*) FROM backup_20251018.evaluacion_fotos;

SELECT @current_rep = COUNT(*) FROM reparacion_fotos;
SELECT @backup_rep = COUNT(*) FROM backup_20251018.reparacion_fotos;

PRINT 'files_documentos:';
PRINT '  Actual: ' + CAST(@current_docs AS VARCHAR) + ' registros → Será: ' + CAST(@backup_docs AS VARCHAR) + ' registros';
PRINT '  Diferencia: ' + CAST(@current_docs - @backup_docs AS VARCHAR);

PRINT '';

PRINT 'evaluacion_fotos:';
PRINT '  Actual: ' + CAST(@current_eval AS VARCHAR) + ' registros → Será: ' + CAST(@backup_eval AS VARCHAR) + ' registros';
PRINT '  Diferencia: ' + CAST(@current_eval - @backup_eval AS VARCHAR);

PRINT '';

PRINT 'reparacion_fotos:';
PRINT '  Actual: ' + CAST(@current_rep AS VARCHAR) + ' registros → Será: ' + CAST(@backup_rep AS VARCHAR) + ' registros';
PRINT '  Diferencia: ' + CAST(@current_rep - @backup_rep AS VARCHAR);

PRINT '';
PRINT '════════════════════════════════════════════════════════════';
PRINT '';

-- ============================================
-- CONFIRMACIÓN INTERACTIVA (comentado por defecto)
-- ============================================
/*
PRINT '¿Estás seguro de que quieres continuar?';
PRINT 'Descomenta la sección de ROLLBACK más abajo para ejecutar.';
PRINT '';
RAISERROR('Rollback detenido por seguridad. Lee las instrucciones.', 16, 1);
RETURN;
*/

-- ============================================
-- ROLLBACK - DESCOMENTA ESTA SECCIÓN PARA EJECUTAR
-- ============================================

/*
PRINT '';
PRINT '🔄 INICIANDO ROLLBACK...';
PRINT '';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ============================================
    -- 1. RESTAURAR files_documentos
    -- ============================================
    PRINT '1️⃣  Restaurando files_documentos...';
    
    -- Deshabilitar constraints temporalmente
    ALTER TABLE evaluacion_fotos NOCHECK CONSTRAINT ALL;
    ALTER TABLE reparacion_fotos NOCHECK CONSTRAINT ALL;
    
    TRUNCATE TABLE files_documentos;
    
    INSERT INTO files_documentos
    SELECT * FROM backup_20251018.files_documentos;
    
    DECLARE @restored_docs INT = @@ROWCOUNT;
    PRINT '✅ Restaurados ' + CAST(@restored_docs AS VARCHAR) + ' registros';
    PRINT '';

    -- ============================================
    -- 2. RESTAURAR evaluacion_fotos
    -- ============================================
    PRINT '2️⃣  Restaurando evaluacion_fotos...';
    
    DELETE FROM evaluacion_fotos;
    
    INSERT INTO evaluacion_fotos
    SELECT * FROM backup_20251018.evaluacion_fotos;
    
    DECLARE @restored_eval INT = @@ROWCOUNT;
    PRINT '✅ Restaurados ' + CAST(@restored_eval AS VARCHAR) + ' registros';
    PRINT '';

    -- ============================================
    -- 3. RESTAURAR reparacion_fotos
    -- ============================================
    PRINT '3️⃣  Restaurando reparacion_fotos...';
    
    DELETE FROM reparacion_fotos;
    
    INSERT INTO reparacion_fotos
    SELECT * FROM backup_20251018.reparacion_fotos;
    
    DECLARE @restored_rep INT = @@ROWCOUNT;
    PRINT '✅ Restaurados ' + CAST(@restored_rep AS VARCHAR) + ' registros';
    PRINT '';

    -- ============================================
    -- 4. REHABILITAR CONSTRAINTS
    -- ============================================
    PRINT '4️⃣  Rehabilitando constraints...';
    
    ALTER TABLE evaluacion_fotos WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE reparacion_fotos WITH CHECK CHECK CONSTRAINT ALL;
    
    PRINT '✅ Constraints rehabilitados';
    PRINT '';

    -- ============================================
    -- 5. COMMIT TRANSACTION
    -- ============================================
    COMMIT TRANSACTION;
    
    PRINT '';
    PRINT '╔═══════════════════════════════════════════════════════════╗';
    PRINT '║  ✅ ROLLBACK COMPLETADO EXITOSAMENTE  ✅                  ║';
    PRINT '╚═══════════════════════════════════════════════════════════╝';
    PRINT '';
    PRINT 'Registros restaurados:';
    PRINT '  - files_documentos: ' + CAST(@restored_docs AS VARCHAR);
    PRINT '  - evaluacion_fotos: ' + CAST(@restored_eval AS VARCHAR);
    PRINT '  - reparacion_fotos: ' + CAST(@restored_rep AS VARCHAR);
    PRINT '';
    PRINT '👍 La base de datos ha sido restaurada al estado del backup.';
    PRINT '';
    PRINT '⚠️  IMPORTANTE: Debes restaurar también los archivos físicos';
    PRINT '   desde el directorio de backup usando el script PowerShell.';
    PRINT '';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT '';
    PRINT '❌ ❌ ❌  ERROR EN ROLLBACK  ❌ ❌ ❌';
    PRINT '';
    PRINT 'Número de Error: ' + CAST(ERROR_NUMBER() AS VARCHAR);
    PRINT 'Mensaje: ' + ERROR_MESSAGE();
    PRINT 'Línea: ' + CAST(ERROR_LINE() AS VARCHAR);
    PRINT '';
    PRINT 'La transacción fue revertida. No se realizaron cambios.';
    PRINT '';
    
    -- Rehabilitar constraints en caso de error
    ALTER TABLE evaluacion_fotos WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE reparacion_fotos WITH CHECK CHECK CONSTRAINT ALL;
    
    THROW;
END CATCH
*/

-- ============================================
-- INSTRUCCIONES DE USO
-- ============================================
PRINT '';
PRINT '📖 INSTRUCCIONES PARA EJECUTAR ROLLBACK:';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '';
PRINT '1. Revisa cuidadosamente el PREVIEW DE CAMBIOS arriba.';
PRINT '';
PRINT '2. Si estás seguro, descomenta la sección marcada como:';
PRINT '   /* ROLLBACK - DESCOMENTA ESTA SECCIÓN PARA EJECUTAR */';
PRINT '';
PRINT '3. Ejecuta el script completo.';
PRINT '';
PRINT '4. Restaura los archivos físicos ejecutando:';
PRINT '   restore_uploads_backup.ps1';
PRINT '';
PRINT '5. Verifica que todo funcione correctamente.';
PRINT '';
PRINT '════════════════════════════════════════════════════════════';
GO
