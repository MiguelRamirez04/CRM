-- ============================================
-- VALIDACIÓN DE INTEGRIDAD - FOTOS
-- Fecha: 2025-10-18
-- Propósito: Validar integridad antes de refactorización
-- ============================================

USE [tu_base_de_datos]; -- Cambiar por el nombre real
GO

PRINT '╔═══════════════════════════════════════════════════════════╗';
PRINT '║  VALIDACIÓN DE INTEGRIDAD - SISTEMA DE FOTOS             ║';
PRINT '║  Fecha: 2025-10-18                                        ║';
PRINT '╚═══════════════════════════════════════════════════════════╝';
PRINT '';

-- ============================================
-- 1. FOTOS HUÉRFANAS EN EVALUACION_FOTOS
-- ============================================
PRINT '1️⃣  VALIDACIÓN: evaluacion_fotos';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

DECLARE @huerfanas_eval INT;
SELECT @huerfanas_eval = COUNT(*)
FROM evaluacion_fotos ef
LEFT JOIN files_documentos fd ON ef.documento_id = fd.id
WHERE fd.id IS NULL;

IF @huerfanas_eval = 0
BEGIN
    PRINT '✅ OK: No hay fotos huérfanas en evaluacion_fotos';
END
ELSE
BEGIN
    PRINT '⚠️  ADVERTENCIA: ' + CAST(@huerfanas_eval AS VARCHAR) + ' fotos huérfanas encontradas';
    
    -- Mostrar detalle
    SELECT 
        ef.id AS foto_id,
        ef.detalle_id,
        ef.documento_id AS documento_id_inexistente,
        ef.tipo,
        ef.descripcion,
        ef.creado_en
    FROM evaluacion_fotos ef
    LEFT JOIN files_documentos fd ON ef.documento_id = fd.id
    WHERE fd.id IS NULL
    ORDER BY ef.creado_en DESC;
END
PRINT '';

-- ============================================
-- 2. FOTOS HUÉRFANAS EN REPARACION_FOTOS
-- ============================================
PRINT '2️⃣  VALIDACIÓN: reparacion_fotos';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

DECLARE @huerfanas_rep INT;
SELECT @huerfanas_rep = COUNT(*)
FROM reparacion_fotos rf
LEFT JOIN files_documentos fd ON rf.documento_id = fd.id
WHERE fd.id IS NULL;

IF @huerfanas_rep = 0
BEGIN
    PRINT '✅ OK: No hay fotos huérfanas en reparacion_fotos';
END
ELSE
BEGIN
    PRINT '⚠️  ADVERTENCIA: ' + CAST(@huerfanas_rep AS VARCHAR) + ' fotos huérfanas encontradas';
    
    -- Mostrar detalle
    SELECT 
        rf.id AS foto_id,
        rf.reparacion_id,
        rf.documento_id AS documento_id_inexistente,
        rf.etapa,
        rf.descripcion,
        rf.creado_en
    FROM reparacion_fotos rf
    LEFT JOIN files_documentos fd ON rf.documento_id = fd.id
    WHERE fd.id IS NULL
    ORDER BY rf.creado_en DESC;
END
PRINT '';

-- ============================================
-- 3. DOCUMENTOS SIN RELACIÓN
-- ============================================
PRINT '3️⃣  VALIDACIÓN: Documentos sin relación en tablas de fotos';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- Documentos Evaluacion sin relación
DECLARE @docs_eval_sin_rel INT;
SELECT @docs_eval_sin_rel = COUNT(*)
FROM files_documentos fd
WHERE fd.entidad_tipo = 'Evaluacion'
  AND fd.activo = 1
  AND NOT EXISTS (
      SELECT 1 FROM evaluacion_fotos ef 
      WHERE ef.documento_id = fd.id
  );

IF @docs_eval_sin_rel = 0
BEGIN
    PRINT '✅ OK: Todos los documentos de Evaluacion tienen relación';
END
ELSE
BEGIN
    PRINT '⚠️  ADVERTENCIA: ' + CAST(@docs_eval_sin_rel AS VARCHAR) + ' documentos de Evaluacion sin relación';
    
    SELECT 
        fd.id AS documento_id,
        fd.entidad_id,
        fd.nombre_archivo,
        fd.creado_en,
        'Evaluacion sin relación' AS estado
    FROM files_documentos fd
    WHERE fd.entidad_tipo = 'Evaluacion'
      AND fd.activo = 1
      AND NOT EXISTS (
          SELECT 1 FROM evaluacion_fotos ef 
          WHERE ef.documento_id = fd.id
      )
    ORDER BY fd.creado_en DESC;
END

-- Documentos Reparacion sin relación
DECLARE @docs_rep_sin_rel INT;
SELECT @docs_rep_sin_rel = COUNT(*)
FROM files_documentos fd
WHERE fd.entidad_tipo = 'Reparacion'
  AND fd.activo = 1
  AND NOT EXISTS (
      SELECT 1 FROM reparacion_fotos rf 
      WHERE rf.documento_id = fd.id
  );

IF @docs_rep_sin_rel = 0
BEGIN
    PRINT '✅ OK: Todos los documentos de Reparacion tienen relación';
END
ELSE
BEGIN
    PRINT '⚠️  ADVERTENCIA: ' + CAST(@docs_rep_sin_rel AS VARCHAR) + ' documentos de Reparacion sin relación';
    
    SELECT 
        fd.id AS documento_id,
        fd.entidad_id,
        fd.nombre_archivo,
        fd.creado_en,
        'Reparacion sin relación' AS estado
    FROM files_documentos fd
    WHERE fd.entidad_tipo = 'Reparacion'
      AND fd.activo = 1
      AND NOT EXISTS (
          SELECT 1 FROM reparacion_fotos rf 
          WHERE rf.documento_id = fd.id
      )
    ORDER BY fd.creado_en DESC;
END
PRINT '';

-- ============================================
-- 4. CONSISTENCIA DE ENTIDAD_ID
-- ============================================
PRINT '4️⃣  VALIDACIÓN: Consistencia de entidad_id (FK lógicas)';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- Evaluaciones con documento pero detalle inexistente
DECLARE @docs_eval_detalle_invalido INT;
SELECT @docs_eval_detalle_invalido = COUNT(*)
FROM files_documentos fd
WHERE fd.entidad_tipo = 'Evaluacion'
  AND fd.activo = 1
  AND NOT EXISTS (
      SELECT 1 FROM evaluaciones_detalles ed 
      WHERE ed.id = fd.entidad_id
  );

IF @docs_eval_detalle_invalido = 0
BEGIN
    PRINT '✅ OK: Todos los documentos de Evaluacion tienen detalle válido';
END
ELSE
BEGIN
    PRINT '⚠️  ADVERTENCIA: ' + CAST(@docs_eval_detalle_invalido AS VARCHAR) + ' documentos con detalle_id inexistente';
    
    SELECT 
        fd.id AS documento_id,
        fd.entidad_id AS detalle_id_inexistente,
        fd.nombre_archivo,
        fd.creado_en
    FROM files_documentos fd
    WHERE fd.entidad_tipo = 'Evaluacion'
      AND fd.activo = 1
      AND NOT EXISTS (
          SELECT 1 FROM evaluaciones_detalles ed 
          WHERE ed.id = fd.entidad_id
      )
    ORDER BY fd.creado_en DESC;
END

-- Reparaciones con documento pero reparación inexistente
DECLARE @docs_rep_reparacion_invalida INT;
SELECT @docs_rep_reparacion_invalida = COUNT(*)
FROM files_documentos fd
WHERE fd.entidad_tipo = 'Reparacion'
  AND fd.activo = 1
  AND NOT EXISTS (
      SELECT 1 FROM reparaciones r 
      WHERE r.id = fd.entidad_id
  );

IF @docs_rep_reparacion_invalida = 0
BEGIN
    PRINT '✅ OK: Todos los documentos de Reparacion tienen reparación válida';
END
ELSE
BEGIN
    PRINT '⚠️  ADVERTENCIA: ' + CAST(@docs_rep_reparacion_invalida AS VARCHAR) + ' documentos con reparacion_id inexistente';
    
    SELECT 
        fd.id AS documento_id,
        fd.entidad_id AS reparacion_id_inexistente,
        fd.nombre_archivo,
        fd.creado_en
    FROM files_documentos fd
    WHERE fd.entidad_tipo = 'Reparacion'
      AND fd.activo = 1
      AND NOT EXISTS (
          SELECT 1 FROM reparaciones r 
          WHERE r.id = fd.entidad_id
      )
    ORDER BY fd.creado_en DESC;
END
PRINT '';

-- ============================================
-- 5. ESTADÍSTICAS GENERALES
-- ============================================
PRINT '5️⃣  ESTADÍSTICAS GENERALES';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

SELECT 
    entidad_tipo AS 'Tipo Entidad',
    COUNT(*) AS 'Total Documentos',
    SUM(CASE WHEN activo = 1 THEN 1 ELSE 0 END) AS 'Activos',
    SUM(CASE WHEN activo = 0 THEN 1 ELSE 0 END) AS 'Inactivos',
    CAST(SUM(tamano_bytes) / 1024.0 / 1024.0 AS DECIMAL(10,2)) AS 'Tamaño Total (MB)',
    CAST(AVG(tamano_bytes) / 1024.0 AS DECIMAL(10,2)) AS 'Tamaño Promedio (KB)'
FROM files_documentos
WHERE entidad_tipo IN ('Evaluacion', 'Reparacion')
GROUP BY entidad_tipo
ORDER BY entidad_tipo;

PRINT '';

-- Resumen de tablas de fotos
SELECT 
    'evaluacion_fotos' AS 'Tabla',
    COUNT(*) AS 'Total Registros',
    MIN(creado_en) AS 'Primer Registro',
    MAX(creado_en) AS 'Último Registro'
FROM evaluacion_fotos
UNION ALL
SELECT 
    'reparacion_fotos',
    COUNT(*),
    MIN(creado_en),
    MAX(creado_en)
FROM reparacion_fotos;

PRINT '';

-- ============================================
-- 6. RESUMEN FINAL
-- ============================================
PRINT '';
PRINT '╔═══════════════════════════════════════════════════════════╗';
PRINT '║  RESUMEN DE VALIDACIÓN                                    ║';
PRINT '╚═══════════════════════════════════════════════════════════╝';

DECLARE @total_issues INT = @huerfanas_eval + @huerfanas_rep + 
                            @docs_eval_sin_rel + @docs_rep_sin_rel +
                            @docs_eval_detalle_invalido + @docs_rep_reparacion_invalida;

PRINT 'Fotos huérfanas (evaluacion_fotos): ' + CAST(@huerfanas_eval AS VARCHAR);
PRINT 'Fotos huérfanas (reparacion_fotos): ' + CAST(@huerfanas_rep AS VARCHAR);
PRINT 'Documentos sin relación (Evaluacion): ' + CAST(@docs_eval_sin_rel AS VARCHAR);
PRINT 'Documentos sin relación (Reparacion): ' + CAST(@docs_rep_sin_rel AS VARCHAR);
PRINT 'Documentos con detalle_id inválido: ' + CAST(@docs_eval_detalle_invalido AS VARCHAR);
PRINT 'Documentos con reparacion_id inválido: ' + CAST(@docs_rep_reparacion_invalida AS VARCHAR);
PRINT '';
PRINT 'TOTAL DE INCONSISTENCIAS: ' + CAST(@total_issues AS VARCHAR);
PRINT '';

IF @total_issues = 0
BEGIN
    PRINT '✅ ✅ ✅  VALIDACIÓN EXITOSA - SIN INCONSISTENCIAS  ✅ ✅ ✅';
    PRINT '';
    PRINT '👍 La base de datos está lista para la refactorización.';
END
ELSE
BEGIN
    PRINT '⚠️  ⚠️  ⚠️   SE ENCONTRARON INCONSISTENCIAS   ⚠️  ⚠️  ⚠️ ';
    PRINT '';
    PRINT '❌ Se recomienda resolver las inconsistencias antes de continuar.';
    PRINT '   Revisa los detalles arriba para cada tipo de problema.';
END

PRINT '';
PRINT '════════════════════════════════════════════════════════════';
PRINT 'Validación completada: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '════════════════════════════════════════════════════════════';
GO
