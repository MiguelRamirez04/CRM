# 🔍 FASE 1: PREPARACIÓN Y ANÁLISIS - Reporte de Auditoría

**Fecha:** 2025-10-18  
**Proyecto:** Integración FileStorageService con Tablas de Fotos  
**Estado:** ✅ En Progreso

---

## 📊 TAREA 1.1: AUDITORÍA DE CÓDIGO EXISTENTE

### **FotosEvaluacionService - Análisis Completo**

#### **Métodos Identificados:**

| Método | Líneas | Funcionalidad | Estado |
|--------|--------|---------------|--------|
| `GetAllFotosAsync()` | ~15 | Lista todas las fotos | ✅ Simple (mantener) |
| `GetFotoByIdAsync()` | ~20 | Obtiene foto por ID | ✅ Simple (mantener) |
| `CreateFotoAsync()` | ~150 | **DUPLICA FileStorageService** | ⚠️ REFACTORIZAR |
| `UpdateFotoAsync()` | ~30 | Actualiza metadatos | ✅ Específico del dominio |
| `DeleteFotoAsync()` | ~40 | **DUPLICA lógica de eliminación** | ⚠️ REFACTORIZAR |
| `DownloadFotoAsync()` | ~35 | **DUPLICA lógica de descarga** | ⚠️ REFACTORIZAR |
| `GetFotosByDetalleIdAsync()` | ~25 | Lista fotos por detalle | ✅ Específico del dominio |
| `SanitizeFileName()` | ~15 | Limpia nombres de archivo | ❌ ELIMINAR (duplicado) |

**Total Líneas:** ~330  
**Líneas a Eliminar:** ~200 (60%)  
**Líneas a Mantener:** ~130 (lógica de negocio específica)

#### **Lógica Común (Duplicada con FileStorageService):**

```csharp
// ❌ DUPLICADO 1: Validación de archivos
if (requestDto.Archivo == null || requestDto.Archivo.Length == 0)
    throw new ArgumentException("El archivo es requerido...");

if (!_imageProcessing.IsValidImage(requestDto.Archivo))
    throw new ArgumentException("Solo se permiten imágenes...");

var maxFileSizeBytes = _maxFileSizeMB * 1024 * 1024;
if (requestDto.Archivo.Length > maxFileSizeBytes)
    throw new ArgumentException($"El archivo no puede exceder...");

// ❌ DUPLICADO 2: Conversión a WebP
var webpFileName = $"{Guid.NewGuid()}_{sanitizedFileName}.webp";
var webpFilePath = Path.Combine(_uploadPath, webpFileName);

using (var inputStream = requestDto.Archivo.OpenReadStream())
{
    (tamanoBytes, ancho, alto) = await _imageProcessing.ConvertToWebPAsync(
        inputStream, webpFilePath, _webpQuality, _maxImageWidth, _maxImageHeight);
}

// ❌ DUPLICADO 3: Creación de metadatos
var metadatos = new {
    ArchivoOriginal = requestDto.Archivo.FileName,
    TamanoOriginal = requestDto.Archivo.Length,
    ContentTypeOriginal = requestDto.Archivo.ContentType,
    Ancho = ancho,
    Alto = alto,
    CalidadWebP = _webpQuality,
    FechaConversion = DateTime.UtcNow
};

// ❌ DUPLICADO 4: Creación en files_documentos
var documento = new FilesDocumento {
    CreadoPorUsuarioId = usuarioId,
    CreadoEn = DateTime.UtcNow,
    EntidadTipo = "Evaluacion",
    EntidadId = requestDto.DetalleId,
    NombreArchivo = webpFileName,
    RutaAlmacenamiento = webpFilePath,
    MimeType = "image/webp",
    TamanoBytes = tamanoBytes,
    MetadatosJson = metadatosJson
};
_writeContext.Documentos.Add(documento);
await _writeContext.SaveChangesAsync();
```

#### **Lógica Específica (Mantener):**

```csharp
// ✅ ESPECÍFICO: Validar que el detalle existe
var detalleExists = await _readonlyContext.EvaluacionesDetalles
    .AnyAsync(d => d.Id == requestDto.DetalleId);
if (!detalleExists)
    throw new KeyNotFoundException($"Detalle de evaluación con ID {requestDto.DetalleId} no encontrado.");

// ✅ ESPECÍFICO: Crear relación en evaluacion_fotos
var nuevaFoto = new EvaluacionFoto {
    DetalleId = requestDto.DetalleId,
    DocumentoId = documento.Id,
    Tipo = requestDto.Tipo,
    Descripcion = requestDto.Descripcion,
    CreadoEn = DateTime.UtcNow
};
_writeContext.EvaluacionesFotos.Add(nuevaFoto);
await _writeContext.SaveChangesAsync();

// ✅ ESPECÍFICO: Mapeo a DTO de respuesta
return new EvaluacionFotoResponseDto {
    Id = nuevaFoto.Id,
    DetalleId = nuevaFoto.DetalleId,
    DocumentoId = nuevaFoto.DocumentoId,
    // ...
};
```

---

### **ReparacionFotoService - Análisis Completo**

#### **Métodos Identificados:**

| Método | Líneas | Funcionalidad | Estado |
|--------|--------|---------------|--------|
| `UploadFotoAsync()` | ~150 | **DUPLICA FileStorageService** | ⚠️ REFACTORIZAR |
| `GetFotosByReparacionIdAsync()` | ~25 | Lista fotos por reparación | ✅ Específico del dominio |
| `GetFotoByIdAsync()` | ~20 | Obtiene foto por ID | ✅ Simple (mantener) |
| `DownloadFotoAsync()` | ~35 | **DUPLICA lógica de descarga** | ⚠️ REFACTORIZAR |
| `DeleteFotoAsync()` | ~40 | **DUPLICA lógica de eliminación** | ⚠️ REFACTORIZAR |
| `UpdateFotoMetadataAsync()` | ~30 | Actualiza metadatos | ✅ Específico del dominio |
| `SanitizeFileName()` | ~15 | Limpia nombres de archivo | ❌ ELIMINAR (duplicado) |

**Total Líneas:** ~315  
**Líneas a Eliminar:** ~190 (60%)  
**Líneas a Mantener:** ~125 (lógica de negocio específica)

#### **Diferencias con FotosEvaluacionService:**

| Aspecto | FotosEvaluacionService | ReparacionFotoService |
|---------|------------------------|----------------------|
| Entidad padre | `evaluaciones_detalles` | `reparaciones` |
| Tabla de relación | `evaluacion_fotos` | `reparacion_fotos` |
| Campo específico | `tipo` (FotoAntes, etc.) | `etapa` (Recibido, etc.) |
| Directorio upload | `CRM/uploads/evaluaciones/` | `CRM/uploads/reparaciones/` |
| Validación padre | Validar `detalle_id` | Validar `reparacion_id` |

**Conclusión:** 🎯 Ambos servicios tienen **exactamente el mismo patrón** de duplicación.

---

## 📊 TAREA 1.2: VALIDACIÓN DE INTEGRIDAD DE BASE DE DATOS

### **Script SQL de Validación:**

```sql
-- ============================================
-- VALIDACIÓN DE INTEGRIDAD - FOTOS
-- ============================================

-- 1. Verificar fotos huérfanas en evaluacion_fotos
PRINT '=== VALIDACIÓN: evaluacion_fotos ===';

SELECT 
    'Fotos Huérfanas (sin documento)' AS Tipo,
    COUNT(*) AS Total
FROM evaluacion_fotos ef
LEFT JOIN files_documentos fd ON ef.documento_id = fd.id
WHERE fd.id IS NULL;

-- Detalle de fotos huérfanas
SELECT 
    ef.id AS foto_id,
    ef.detalle_id,
    ef.documento_id,
    ef.tipo,
    ef.creado_en
FROM evaluacion_fotos ef
LEFT JOIN files_documentos fd ON ef.documento_id = fd.id
WHERE fd.id IS NULL;

-- 2. Verificar fotos huérfanas en reparacion_fotos
PRINT '=== VALIDACIÓN: reparacion_fotos ===';

SELECT 
    'Fotos Huérfanas (sin documento)' AS Tipo,
    COUNT(*) AS Total
FROM reparacion_fotos rf
LEFT JOIN files_documentos fd ON rf.documento_id = fd.id
WHERE fd.id IS NULL;

-- Detalle de fotos huérfanas
SELECT 
    rf.id AS foto_id,
    rf.reparacion_id,
    rf.documento_id,
    rf.etapa,
    rf.creado_en
FROM reparacion_fotos rf
LEFT JOIN files_documentos fd ON rf.documento_id = fd.id
WHERE fd.id IS NULL;

-- 3. Verificar documentos sin relación (posible limpieza)
PRINT '=== VALIDACIÓN: files_documentos sin relación ===';

SELECT 
    'Documentos Evaluacion sin relación' AS Tipo,
    COUNT(*) AS Total
FROM files_documentos fd
WHERE fd.entidad_tipo = 'Evaluacion'
  AND NOT EXISTS (
      SELECT 1 FROM evaluacion_fotos ef 
      WHERE ef.documento_id = fd.id
  );

SELECT 
    'Documentos Reparacion sin relación' AS Tipo,
    COUNT(*) AS Total
FROM files_documentos fd
WHERE fd.entidad_tipo = 'Reparacion'
  AND NOT EXISTS (
      SELECT 1 FROM reparacion_fotos rf 
      WHERE rf.documento_id = fd.id
  );

-- 4. Verificar archivos físicos vs base de datos
SELECT 
    fd.id,
    fd.nombre_archivo,
    fd.ruta_almacenamiento,
    fd.entidad_tipo,
    fd.activo,
    fd.creado_en
FROM files_documentos fd
WHERE fd.entidad_tipo IN ('Evaluacion', 'Reparacion')
ORDER BY fd.creado_en DESC;

-- 5. Estadísticas generales
SELECT 
    entidad_tipo,
    COUNT(*) AS total_documentos,
    SUM(CASE WHEN activo = 1 THEN 1 ELSE 0 END) AS activos,
    SUM(CASE WHEN activo = 0 THEN 1 ELSE 0 END) AS inactivos,
    SUM(tamano_bytes) AS tamano_total_bytes,
    AVG(tamano_bytes) AS tamano_promedio_bytes
FROM files_documentos
WHERE entidad_tipo IN ('Evaluacion', 'Reparacion')
GROUP BY entidad_tipo;

-- 6. Verificar consistencia de entidad_id
PRINT '=== VALIDACIÓN: Consistencia de entidad_id ===';

-- Evaluaciones con documento pero detalle inexistente
SELECT 
    fd.id AS documento_id,
    fd.entidad_id AS detalle_id_referenciado,
    fd.creado_en
FROM files_documentos fd
WHERE fd.entidad_tipo = 'Evaluacion'
  AND NOT EXISTS (
      SELECT 1 FROM evaluaciones_detalles ed 
      WHERE ed.id = fd.entidad_id
  );

-- Reparaciones con documento pero reparación inexistente
SELECT 
    fd.id AS documento_id,
    fd.entidad_id AS reparacion_id_referenciado,
    fd.creado_en
FROM files_documentos fd
WHERE fd.entidad_tipo = 'Reparacion'
  AND NOT EXISTS (
      SELECT 1 FROM reparaciones r 
      WHERE r.id = fd.entidad_id
  );
```

### **Resultado Esperado:**

```
RESULTADO VALIDACIÓN (esperado):
✅ Fotos Huérfanas en evaluacion_fotos: 0
✅ Fotos Huérfanas en reparacion_fotos: 0
✅ Documentos sin relación: 0 (o mínimo)
✅ Archivos físicos coinciden con BD
✅ Consistencia de entidad_id: OK
```

---

## 🗂️ TAREA 1.3: BACKUP DE SEGURIDAD

### **Script de Backup SQL Server:**

```sql
-- ============================================
-- BACKUP DE TABLAS ANTES DE REFACTORIZACIÓN
-- Fecha: 2025-10-18
-- ============================================

-- 1. Crear esquema temporal para backups
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'backup_20251018')
BEGIN
    EXEC('CREATE SCHEMA backup_20251018');
END
GO

-- 2. Backup de files_documentos
SELECT *
INTO backup_20251018.files_documentos
FROM files_documentos;
GO

PRINT 'Backup files_documentos: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' registros';

-- 3. Backup de evaluacion_fotos
SELECT *
INTO backup_20251018.evaluacion_fotos
FROM evaluacion_fotos;
GO

PRINT 'Backup evaluacion_fotos: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' registros';

-- 4. Backup de reparacion_fotos
SELECT *
INTO backup_20251018.reparacion_fotos
FROM reparacion_fotos;
GO

PRINT 'Backup reparacion_fotos: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' registros';

-- 5. Verificar backups creados
SELECT 
    t.name AS tabla_backup,
    p.rows AS total_registros,
    CAST(ROUND(((SUM(a.used_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS tamano_mb
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE SCHEMA_NAME(t.schema_id) = 'backup_20251018'
  AND p.index_id IN (0,1)
GROUP BY t.name, p.rows
ORDER BY t.name;
GO

-- ============================================
-- SCRIPT DE ROLLBACK (si es necesario)
-- ============================================
/*
-- ADVERTENCIA: Solo ejecutar si necesitas revertir cambios

-- 1. Restaurar files_documentos
TRUNCATE TABLE files_documentos;
INSERT INTO files_documentos
SELECT * FROM backup_20251018.files_documentos;

-- 2. Restaurar evaluacion_fotos
TRUNCATE TABLE evaluacion_fotos;
INSERT INTO evaluacion_fotos
SELECT * FROM backup_20251018.evaluacion_fotos;

-- 3. Restaurar reparacion_fotos
TRUNCATE TABLE reparacion_fotos;
INSERT INTO reparacion_fotos
SELECT * FROM backup_20251018.reparacion_fotos;

PRINT 'Rollback completado exitosamente';
*/
```

### **Backup de Directorio de Uploads (PowerShell):**

```powershell
# ============================================
# BACKUP DE ARCHIVOS FÍSICOS
# Fecha: 2025-10-18
# ============================================

$backupDate = Get-Date -Format "yyyyMMdd_HHmmss"
$sourceDir = "C:\Users\adria\source\repos\fullstack_cabs\back_cabs\CRM\uploads"
$backupDir = "C:\Users\adria\source\repos\fullstack_cabs\back_cabs\CRM\uploads_backup_$backupDate"

Write-Host "=== INICIO DE BACKUP ===" -ForegroundColor Green
Write-Host "Origen: $sourceDir" -ForegroundColor Cyan
Write-Host "Destino: $backupDir" -ForegroundColor Cyan

# Crear directorio de backup
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null

# Copiar archivos recursivamente
Copy-Item -Path "$sourceDir\*" -Destination $backupDir -Recurse -Force

# Verificar backup
$sourceCount = (Get-ChildItem -Path $sourceDir -Recurse -File).Count
$backupCount = (Get-ChildItem -Path $backupDir -Recurse -File).Count
$backupSize = (Get-ChildItem -Path $backupDir -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB

Write-Host "`n=== RESULTADO DEL BACKUP ===" -ForegroundColor Green
Write-Host "Archivos originales: $sourceCount" -ForegroundColor Yellow
Write-Host "Archivos respaldados: $backupCount" -ForegroundColor Yellow
Write-Host "Tamaño del backup: $([math]::Round($backupSize, 2)) MB" -ForegroundColor Yellow

if ($sourceCount -eq $backupCount) {
    Write-Host "`n✅ BACKUP COMPLETADO EXITOSAMENTE" -ForegroundColor Green
} else {
    Write-Host "`n⚠️ ADVERTENCIA: Diferencia en cantidad de archivos" -ForegroundColor Red
}

# Listar estructura del backup
Write-Host "`n=== ESTRUCTURA DEL BACKUP ===" -ForegroundColor Cyan
Get-ChildItem -Path $backupDir -Recurse -Directory | 
    Select-Object FullName, @{Name='Files';Expression={(Get-ChildItem $_.FullName -File).Count}} |
    Format-Table -AutoSize
```

---

## 📊 RESUMEN DE HALLAZGOS - FASE 1

### **Código Duplicado Identificado:**

| Componente | FotosEvaluacionService | ReparacionFotoService | Total |
|------------|------------------------|----------------------|-------|
| Validación de archivos | ~40 líneas | ~40 líneas | 80 líneas |
| Conversión WebP | ~60 líneas | ~60 líneas | 120 líneas |
| Creación metadatos | ~30 líneas | ~30 líneas | 60 líneas |
| Guardado en BD | ~40 líneas | ~40 líneas | 80 líneas |
| Descarga archivos | ~35 líneas | ~35 líneas | 70 líneas |
| Eliminación archivos | ~40 líneas | ~40 líneas | 80 líneas |
| **TOTAL** | **~245 líneas** | **~245 líneas** | **~490 líneas** |

**🎯 Reducción esperada: ~490 líneas de código duplicado → ~100 líneas (80% menos)**

### **Dependencias a Actualizar:**

#### **FotosEvaluacionService:**
```csharp
// ❌ ELIMINAR
private readonly ImageProcessingService _imageProcessing;
private readonly string _uploadPath;
private readonly int _maxFileSizeMB;
private readonly int _webpQuality;
private readonly int _maxImageWidth;
private readonly int _maxImageHeight;

// ✅ AGREGAR
private readonly IFileStorageService _fileStorageService;
```

#### **ReparacionFotoService:**
```csharp
// ❌ ELIMINAR (mismas dependencias)
// ✅ AGREGAR (mismo cambio)
```

### **Métodos a Refactorizar:**

#### **Alta Prioridad (Duplicación Crítica):**
1. ✅ `CreateFotoAsync()` / `UploadFotoAsync()` - Subida de archivos
2. ✅ `DownloadFotoAsync()` - Descarga de archivos
3. ✅ `DeleteFotoAsync()` - Eliminación de archivos

#### **Media Prioridad (Optimización):**
4. ✅ `GetFotosByDetalleIdAsync()` / `GetFotosByReparacionIdAsync()` - Incluir documento
5. ✅ Eliminar métodos privados duplicados (`SanitizeFileName`, etc.)

#### **Baja Prioridad (Mantener Como Está):**
6. ⏸️ `GetFotoByIdAsync()` - Simple, no requiere cambios
7. ⏸️ `UpdateFotoAsync()` - Lógica específica del dominio

---

## ✅ CHECKLIST FASE 1

### **Tarea 1.1: Auditoría de Código**
- [x] Revisar FotosEvaluacionService completo
- [x] Revisar ReparacionFotoService completo
- [x] Identificar código duplicado (~490 líneas)
- [x] Identificar lógica específica del dominio (~255 líneas)
- [x] Documentar diferencias entre servicios

### **Tarea 1.2: Validación de Base de Datos**
- [x] Crear script SQL de validación
- [ ] **PENDIENTE:** Ejecutar script en base de datos
- [ ] **PENDIENTE:** Documentar resultados de validación
- [ ] **PENDIENTE:** Resolver inconsistencias (si hay)

### **Tarea 1.3: Backup de Seguridad**
- [x] Crear script SQL de backup
- [x] Crear script PowerShell de backup de archivos
- [ ] **PENDIENTE:** Ejecutar backup de base de datos
- [ ] **PENDIENTE:** Ejecutar backup de directorio uploads
- [ ] **PENDIENTE:** Verificar integridad de backups

---

## 🚀 PRÓXIMOS PASOS

### **Acción Inmediata:**
1. ✅ **Ejecutar script de validación SQL** (ver resultados)
2. ✅ **Ejecutar backups** (BD + archivos físicos)
3. ✅ **Verificar que no hay inconsistencias** críticas

### **Siguiente Fase:**
Una vez completados los backups y validaciones:
- ➡️ **FASE 2:** Refactorización de FotosEvaluacionService

---

## 📝 NOTAS ADICIONALES

### **Observaciones Importantes:**

1. **Transacciones:** Ambos servicios usan transacciones explícitas. Debemos mantener esta atomicidad.

2. **Strategy Pattern:** Ambos servicios usan `_writeContext.Database.CreateExecutionStrategy()` para reintentos.

3. **Logging:** Ambos servicios tienen logging detallado que debemos preservar.

4. **Configuración:** Ambos servicios leen de `IConfiguration` (FileStorage:*). Ya está consolidado en FileStorageService.

5. **Validaciones de Negocio:** Cada servicio tiene validaciones específicas que NO deben moverse a FileStorageService:
   - FotosEvaluacion: Validar que `detalle_id` existe
   - ReparacionFoto: Validar que `reparacion_id` existe

---

**Estado:** ✅ Auditoría Completada - Listos para Ejecutar Validaciones y Backups  
**Siguiente:** Ejecutar scripts SQL y PowerShell antes de continuar con Fase 2
