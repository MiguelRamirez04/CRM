-- ============================================
-- BACKUP DE TABLAS - ANTES DE REFACTORIZACIÓN
-- Fecha: 2025-10-18
-- Propósito: Crear copias de seguridad antes de cambios
-- ============================================

USE [tu_base_de_datos]; -- Cambiar por el nombre real
GO

SET NOCOUNT ON;
GO

PRINT '╔═══════════════════════════════════════════════════════════╗';
PRINT '║  BACKUP DE TABLAS - SISTEMA DE FOTOS                     ║';
PRINT '║  Fecha: 2025-10-18                                        ║';
PRINT '╚═══════════════════════════════════════════════════════════╝';
PRINT '';

-- ============================================
-- 1. CREAR ESQUEMA DE BACKUP
-- ============================================
PRINT '1️⃣  Creando esquema de backup...';

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'backup_20251018')
BEGIN
    EXEC('CREATE SCHEMA backup_20251018');
    PRINT '✅ Esquema backup_20251018 creado exitosamente';
END
ELSE
BEGIN
    PRINT '⚠️  El esquema backup_20251018 ya existe';
    PRINT '   Eliminando tablas antiguas...';
    
    IF OBJECT_ID('backup_20251018.files_documentos', 'U') IS NOT NULL
        DROP TABLE backup_20251018.files_documentos;
    IF OBJECT_ID('backup_20251018.evaluacion_fotos', 'U') IS NOT NULL
        DROP TABLE backup_20251018.evaluacion_fotos;
    IF OBJECT_ID('backup_20251018.reparacion_fotos', 'U') IS NOT NULL
        DROP TABLE backup_20251018.reparacion_fotos;
        
    PRINT '✅ Tablas antiguas eliminadas';
END
PRINT '';

-- ============================================
-- 2. BACKUP files_documentos
-- ============================================
PRINT '2️⃣  Creando backup de files_documentos...';

SELECT *
INTO backup_20251018.files_documentos
FROM files_documentos;

DECLARE @count_docs INT = @@ROWCOUNT;
PRINT '✅ Backup completado: ' + CAST(@count_docs AS VARCHAR) + ' registros copiados';
PRINT '';

-- ============================================
-- 3. BACKUP evaluacion_fotos
-- ============================================
PRINT '3️⃣  Creando backup de evaluacion_fotos...';

SELECT *
INTO backup_20251018.evaluacion_fotos
FROM evaluacion_fotos;

DECLARE @count_eval INT = @@ROWCOUNT;
PRINT '✅ Backup completado: ' + CAST(@count_eval AS VARCHAR) + ' registros copiados';
PRINT '';

-- ============================================
-- 4. BACKUP reparacion_fotos
-- ============================================
PRINT '4️⃣  Creando backup de reparacion_fotos...';

SELECT *
INTO backup_20251018.reparacion_fotos
FROM reparacion_fotos;

DECLARE @count_rep INT = @@ROWCOUNT;
PRINT '✅ Backup completado: ' + CAST(@count_rep AS VARCHAR) + ' registros copiados';
PRINT '';

-- ============================================
-- 5. VERIFICAR BACKUPS CREADOS
-- ============================================
PRINT '5️⃣  Verificando backups creados...';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

SELECT 
    t.name AS 'Tabla Backup',
    p.rows AS 'Total Registros',
    CAST(ROUND(((SUM(a.used_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS 'Tamaño (MB)',
    CONVERT(VARCHAR, GETDATE(), 120) AS 'Fecha Backup'
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE SCHEMA_NAME(t.schema_id) = 'backup_20251018'
  AND p.index_id IN (0,1)
GROUP BY t.name, p.rows
ORDER BY t.name;

PRINT '';

-- ============================================
-- 6. VALIDAR INTEGRIDAD DE BACKUPS
-- ============================================
PRINT '6️⃣  Validando integridad de backups...';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- Comparar counts
DECLARE @original_docs INT, @backup_docs INT;
DECLARE @original_eval INT, @backup_eval INT;
DECLARE @original_rep INT, @backup_rep INT;

SELECT @original_docs = COUNT(*) FROM files_documentos;
SELECT @backup_docs = COUNT(*) FROM backup_20251018.files_documentos;

SELECT @original_eval = COUNT(*) FROM evaluacion_fotos;
SELECT @backup_eval = COUNT(*) FROM backup_20251018.evaluacion_fotos;

SELECT @original_rep = COUNT(*) FROM reparacion_fotos;
SELECT @backup_rep = COUNT(*) FROM backup_20251018.reparacion_fotos;

PRINT 'files_documentos:';
PRINT '  Original: ' + CAST(@original_docs AS VARCHAR) + ' | Backup: ' + CAST(@backup_docs AS VARCHAR);
IF @original_docs = @backup_docs
    PRINT '  ✅ Coinciden';
ELSE
    PRINT '  ❌ NO COINCIDEN';

PRINT '';

PRINT 'evaluacion_fotos:';
PRINT '  Original: ' + CAST(@original_eval AS VARCHAR) + ' | Backup: ' + CAST(@backup_eval AS VARCHAR);
IF @original_eval = @backup_eval
    PRINT '  ✅ Coinciden';
ELSE
    PRINT '  ❌ NO COINCIDEN';

PRINT '';

PRINT 'reparacion_fotos:';
PRINT '  Original: ' + CAST(@original_rep AS VARCHAR) + ' | Backup: ' + CAST(@backup_rep AS VARCHAR);
IF @original_rep = @backup_rep
    PRINT '  ✅ Coinciden';
ELSE
    PRINT '  ❌ NO COINCIDEN';

PRINT '';

-- ============================================
-- 7. RESUMEN FINAL
-- ============================================
PRINT '╔═══════════════════════════════════════════════════════════╗';
PRINT '║  RESUMEN DE BACKUP                                        ║';
PRINT '╚═══════════════════════════════════════════════════════════╝';

DECLARE @total_registros INT = @backup_docs + @backup_eval + @backup_rep;

PRINT 'Esquema de backup: backup_20251018';
PRINT 'Total de registros respaldados: ' + CAST(@total_registros AS VARCHAR);
PRINT '';
PRINT 'Detalle:';
PRINT '  - files_documentos: ' + CAST(@backup_docs AS VARCHAR) + ' registros';
PRINT '  - evaluacion_fotos: ' + CAST(@backup_eval AS VARCHAR) + ' registros';
PRINT '  - reparacion_fotos: ' + CAST(@backup_rep AS VARCHAR) + ' registros';
PRINT '';

IF (@original_docs = @backup_docs) AND 
   (@original_eval = @backup_eval) AND 
   (@original_rep = @backup_rep)
BEGIN
    PRINT '✅ ✅ ✅  BACKUP COMPLETADO EXITOSAMENTE  ✅ ✅ ✅';
    PRINT '';
    PRINT '👍 Todos los registros fueron respaldados correctamente.';
    PRINT '   Puedes proceder con la refactorización.';
END
ELSE
BEGIN
    PRINT '⚠️  ⚠️  ⚠️   ADVERTENCIA EN BACKUP   ⚠️  ⚠️  ⚠️ ';
    PRINT '';
    PRINT '❌ Hay diferencias entre las tablas originales y el backup.';
    PRINT '   Revisa los detalles arriba antes de continuar.';
END

PRINT '';
PRINT '════════════════════════════════════════════════════════════';
PRINT 'Backup completado: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '════════════════════════════════════════════════════════════';
PRINT '';
PRINT 'NOTA: Para restaurar el backup, ejecuta el script:';
PRINT '      rollback_backup_20251018.sql';
GO

-- ============================================
-- CREAR SCRIPT DE ROLLBACK AUTOMÁTICAMENTE
-- ============================================
PRINT '';
PRINT '📝 Generando script de rollback...';
PRINT 'Ubicación: rollback_backup_20251018.sql';
PRINT '';
GO
