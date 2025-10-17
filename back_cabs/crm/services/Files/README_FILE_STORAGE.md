# 📁 Sistema de Gestión de Archivos - Documentación Completa

## 📋 Resumen General

Sistema centralizado y robusto para gestionar archivos en el sistema CABs. Soporta imágenes (con conversión automática a WebP) y documentos (PDF, Excel, Word).

---

## 🏗️ Arquitectura

### **Tabla Central: `files_documentos`**

```sql
CREATE TABLE [dbo].[files_documentos](
    [id] [int] IDENTITY(1,1) PRIMARY KEY,
    [creado_por_usuario_id] [int] NOT NULL,
    [creado_en] [datetime2](0) NOT NULL,
    [entidad_tipo] [varchar](50) NOT NULL,
    [entidad_id] [int] NOT NULL,
    [metadatos_json] [nvarchar](max) NULL,
    [tamano_bytes] [bigint] NULL,
    [mimetype] [varchar](100) NULL,
    [ruta_almacenamiento] [nvarchar](500) NOT NULL,
    [nombre_archivo] [nvarchar](255) NOT NULL,
    [activo] [bit] NOT NULL DEFAULT 1,
    [actualizado_en] [datetime2](0) NULL,
    [nombre_original] [nvarchar](255) NULL,
    [checksum_sha256] [varchar](64) NULL
);

-- Índices para optimización
CREATE INDEX IX_files_creado_por ON files_documentos(creado_por_usuario_id);
CREATE INDEX IX_files_entidad ON files_documentos(entidad_tipo, entidad_id);
CREATE INDEX IX_files_activo ON files_documentos(activo) WHERE activo = 1;
CREATE INDEX IX_files_checksum ON files_documentos(checksum_sha256);
CREATE INDEX IX_files_fecha ON files_documentos(creado_en);
CREATE INDEX IX_files_mimetype ON files_documentos(mimetype);
```

### **Patrón Polimórfico**

La tabla usa un patrón polimórfico para asociar archivos a diferentes entidades:
- `entidad_tipo`: Tipo de entidad (Evaluacion, Reparacion, GastoViatico, etc.)
- `entidad_id`: ID de la entidad específica

---

## 🔧 Componentes del Sistema

### **1. Enums**

#### `TipoEntidadDocumento`
```csharp
public enum TipoEntidadDocumento
{
    Evaluacion,    // Solo imágenes → WebP
    Reparacion,    // Solo imágenes → WebP
    GastoViatico,  // PDFs, Excel (facturas)
    OrdenTrabajo,  // PDFs, Excel (cotizaciones)
    Cliente,       // Documentos generales
    Vehiculo       // Documentos generales
}
```

#### `CategoriaDocumento`
```csharp
public enum CategoriaDocumento
{
    FotoAntes, FotoDurante, FotoDespues,
    FotoEvaluacion, Factura, Cotizacion,
    Recibo, Otro
}
```

### **2. Modelo**

```csharp
[Table("files_documentos")]
public class FilesDocumento
{
    public int Id { get; set; }
    public int CreadoPorUsuarioId { get; set; }
    public DateTime CreadoEn { get; set; }
    public string EntidadTipo { get; set; }
    public int EntidadId { get; set; }
    public string? MetadatosJson { get; set; }
    public long? TamanoBytes { get; set; }
    public string? MimeType { get; set; }
    public string RutaAlmacenamiento { get; set; }
    public string NombreArchivo { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? ActualizadoEn { get; set; }
    public string? NombreOriginal { get; set; }
    public string? ChecksumSHA256 { get; set; }
}
```

### **3. Servicio - `FileStorageService`**

#### **Características Principales:**

✅ **Validación Robusta**
- Tamaño máximo: 10MB (configurable)
- Tipos permitidos: JPEG, PNG, WebP, GIF, PDF, Excel, Word
- Validación de MIME type y extensión

✅ **Conversión Automática a WebP**
- Solo para entidades `Evaluacion` y `Reparacion`
- Usa `ImageProcessingService` existente
- Reduce tamaño de archivos significativamente

✅ **Prevención de Duplicados**
- Calcula SHA256 checksum
- Detecta archivos idénticos en la misma entidad

✅ **Organización de Archivos**
```
uploads/
├── evaluacion/
│   ├── guid1.webp
│   └── guid2.webp
├── reparacion/
│   ├── guid3.webp
│   └── guid4.webp
└── gastoviatico/
    ├── guid5.pdf
    └── guid6.xlsx
```

✅ **Soft Delete**
- No elimina archivos físicos
- Marca como `Activo = false`

✅ **Validación de Integridad**
- Verifica checksum SHA256
- Detecta archivos corruptos o modificados

---

## 📡 API REST - Endpoints

### **1. Subir Archivo**

```http
POST /api/Files/upload
Content-Type: multipart/form-data
Authorization: Bearer {token}

FormData:
- Archivo: [file]
- EntidadTipo: "Evaluacion" | "Reparacion" | "GastoViatico" | ...
- EntidadId: 123
- Descripcion: "Foto del daño frontal" (opcional)
- Categoria: "FotoAntes" (opcional)
```

**Respuesta 201 Created:**
```json
{
  "id": 456,
  "nombreArchivo": "abc123.webp",
  "nombreOriginal": "foto_dano.jpg",
  "mimeType": "image/webp",
  "tamanoBytes": 245678,
  "tamanoFormateado": "240 KB",
  "creadoEn": "2025-10-17T14:30:00Z",
  "creadoPorNombre": "Juan Pérez",
  "descripcion": "Foto del daño frontal",
  "urlDescarga": "/api/Files/456/download",
  "esImagen": true,
  "convertidoAWebP": true,
  "categoria": "FotoAntes"
}
```

### **2. Descargar Archivo**

```http
GET /api/Files/{id}/download
```

**Respuesta 200 OK:**
- Stream del archivo
- Headers: `Content-Type`, `Content-Disposition`

### **3. Listar Archivos de una Entidad**

```http
GET /api/Files/entity?EntidadTipo=Evaluacion&EntidadId=123&SoloImagenes=true
```

**Respuesta 200 OK:**
```json
[
  {
    "id": 456,
    "nombreArchivo": "abc123.webp",
    "nombreOriginal": "foto1.jpg",
    "mimeType": "image/webp",
    "tamanoBytes": 245678,
    "tamanoFormateado": "240 KB",
    "creadoEn": "2025-10-17T14:30:00Z",
    "urlDescarga": "/api/Files/456/download",
    "esImagen": true,
    "convertidoAWebP": true
  },
  ...
]
```

### **4. Eliminar Archivo (Soft Delete)**

```http
DELETE /api/Files/{id}
Authorization: Bearer {token}
```

**Respuesta 204 No Content**

### **5. Validar Integridad**

```http
GET /api/Files/{id}/validate
```

**Respuesta 200 OK:**
```json
{
  "id": 456,
  "esValido": true,
  "mensaje": "El archivo es íntegro"
}
```

### **6. Obtener Información de un Archivo**

```http
GET /api/Files/{id}
```

**Respuesta 200 OK:**
```json
{
  "id": 456,
  "nombreArchivo": "abc123.webp",
  "nombreOriginal": "foto_dano.jpg",
  "mimeType": "image/webp",
  "tamanoBytes": 245678,
  "tamanoFormateado": "240 KB",
  "creadoEn": "2025-10-17T14:30:00Z",
  "urlDescarga": "/api/Files/456/download",
  "esImagen": true,
  "convertidoAWebP": true
}
```

---

## 🔄 Flujos de Uso

### **Flujo 1: Subir Foto de Evaluación**

```
1. Frontend: Usuario selecciona imagen JPEG (5MB)
   ↓
2. POST /api/Files/upload
   - Archivo: foto_dano.jpg
   - EntidadTipo: "Evaluacion"
   - EntidadId: 123
   ↓
3. FileStorageService:
   a. Valida tamaño (5MB < 10MB ✓)
   b. Valida tipo MIME (image/jpeg ✓)
   c. Calcula SHA256 checksum
   d. Verifica duplicados en BD
   e. Detecta que es Evaluacion → Convertir a WebP
   f. ImageProcessingService convierte JPEG → WebP
   g. Guarda en /uploads/evaluacion/guid.webp
   h. Crea registro en files_documentos
   ↓
4. Respuesta:
   {
     "id": 456,
     "nombreArchivo": "guid.webp",
     "nombreOriginal": "foto_dano.jpg",
     "mimeType": "image/webp",
     "tamanoBytes": 1200000, // Reducido de 5MB a ~1.2MB
     "convertidoAWebP": true
   }
   ↓
5. Frontend: Muestra imagen usando /api/Files/456/download
```

### **Flujo 2: Subir Factura PDF para Gasto Viático**

```
1. Frontend: Usuario selecciona factura.pdf (2MB)
   ↓
2. POST /api/Files/upload
   - Archivo: factura.pdf
   - EntidadTipo: "GastoViatico"
   - EntidadId: 789
   - Categoria: "Factura"
   ↓
3. FileStorageService:
   a. Valida tamaño (2MB < 10MB ✓)
   b. Valida tipo MIME (application/pdf ✓)
   c. Calcula SHA256 checksum
   d. Verifica duplicados
   e. Detecta que NO es Evaluacion/Reparacion → Guardar original
   f. Guarda en /uploads/gastoviatico/guid.pdf
   g. Crea registro en files_documentos
   ↓
4. Respuesta:
   {
     "id": 457,
     "nombreArchivo": "guid.pdf",
     "nombreOriginal": "factura.pdf",
     "mimeType": "application/pdf",
     "tamanoBytes": 2048000,
     "convertidoAWebP": false,
     "categoria": "Factura"
   }
```

### **Flujo 3: Obtener Todas las Fotos de una Reparación**

```
1. Frontend: GET /api/Files/entity?EntidadTipo=Reparacion&EntidadId=456&SoloImagenes=true
   ↓
2. FileStorageService.GetFilesByEntidadAsync()
   - Filtra por EntidadTipo = "Reparacion"
   - Filtra por EntidadId = 456
   - Filtra por Activo = true
   - Filtra por MimeType startsWith "image/"
   ↓
3. Respuesta: Array de archivos
   [
     { id: 101, nombreArchivo: "guid1.webp", categoria: "FotoAntes" },
     { id: 102, nombreArchivo: "guid2.webp", categoria: "FotoDurante" },
     { id: 103, nombreArchivo: "guid3.webp", categoria: "FotoDespues" }
   ]
   ↓
4. Frontend: Renderiza galería de imágenes
```

---

## 🔗 Integración con Tablas Existentes

### **1. Evaluación Fotos (`evaluacion_fotos`)**

```csharp
[Table("evaluacion_fotos")]
public class EvaluacionFoto
{
    public int Id { get; set; }
    public int DetalleId { get; set; }
    
    [ForeignKey("DocumentoId")]
    public int DocumentoId { get; set; } // FK a files_documentos
    
    public virtual FilesDocumento Documento { get; set; }
    
    public string Tipo { get; set; } // "Antes", "Despues"
    public string? Descripcion { get; set; }
}
```

**Servicio Actualizado:**
```csharp
public async Task<EvaluacionFoto> CreateFotoAsync(
    int detalleId,
    IFormFile archivo,
    string tipo,
    int usuarioId)
{
    // 1. Subir archivo usando servicio genérico
    var documento = await _fileStorage.UploadFileAsync(
        archivo,
        TipoEntidadDocumento.Evaluacion,
        detalleId,
        usuarioId,
        descripcion: $"Foto {tipo}",
        categoria: tipo);
    
    // 2. Crear relación en evaluacion_fotos
    var foto = new EvaluacionFoto
    {
        DetalleId = detalleId,
        DocumentoId = documento.Id,
        Tipo = tipo,
        Descripcion = $"Foto {tipo}"
    };
    
    _writeContext.EvaluacionesFotos.Add(foto);
    await _writeContext.SaveChangesAsync();
    
    return foto;
}
```

### **2. Reparación Fotos (`reparacion_fotos`)**

Similar a evaluación_fotos:

```csharp
[Table("reparacion_fotos")]
public class ReparacionFoto
{
    public int Id { get; set; }
    public int ReparacionId { get; set; }
    
    [ForeignKey("DocumentoId")]
    public int DocumentoId { get; set; } // FK a files_documentos
    
    public virtual FilesDocumento Documento { get; set; }
    
    public string Tipo { get; set; } // "Antes", "Durante", "Despues"
}
```

### **3. Gastos Viáticos (Futuro) - Facturas PDF**

```csharp
[Table("finance_gastos_viaticos")]
public class GastoViatico
{
    public int Id { get; set; }
    // ... otros campos ...
    
    [ForeignKey("FacturaDocumentoId")]
    public int? FacturaDocumentoId { get; set; } // FK a files_documentos
    
    public virtual FilesDocumento? FacturaDocumento { get; set; }
}
```

**Uso:**
```csharp
// Al crear gasto con factura
var documento = await _fileStorage.UploadFileAsync(
    facturaPdf,
    TipoEntidadDocumento.GastoViatico,
    gastoId,
    usuarioId,
    categoria: "Factura");

gasto.FacturaDocumentoId = documento.Id;
await _writeContext.SaveChangesAsync();
```

---

## ⚙️ Configuración en `appsettings.json`

```json
{
  "FileStorage": {
    "UploadPath": "uploads",
    "MaxFileSizeMB": 10,
    "WebPQuality": 80,
    "MaxImageWidth": 1920,
    "MaxImageHeight": 1080
  }
}
```

---

## 🔒 Seguridad

### **Validaciones Implementadas:**

✅ **Tamaño de archivo:** Máximo 10MB
✅ **Tipos MIME:** Solo permitidos (imágenes, PDF, Excel, Word)
✅ **Extensiones:** Doble verificación
✅ **Duplicados:** Detección por SHA256
✅ **Integridad:** Validación de checksum
✅ **Soft Delete:** No se eliminan archivos físicamente
✅ **Autenticación:** Requiere JWT token
✅ **Auditoría:** Registra quién subió/eliminó cada archivo

### **Recomendaciones Adicionales:**

⚠️ Implementar antivirus scanner
⚠️ Rate limiting en endpoints de subida
⚠️ Validación de permisos por entidad
⚠️ Encriptación de archivos sensibles
⚠️ Backup automático de archivos

---

## 📊 Monitoreo y Mantenimiento

### **Queries Útiles:**

```sql
-- Archivos por tipo de entidad
SELECT entidad_tipo, COUNT(*) as total, SUM(tamano_bytes)/1024/1024 as total_mb
FROM files_documentos
WHERE activo = 1
GROUP BY entidad_tipo;

-- Archivos huérfanos (sin entidad válida)
SELECT fd.*
FROM files_documentos fd
LEFT JOIN evaluacion_fotos ef ON fd.id = ef.documento_id
LEFT JOIN reparacion_fotos rf ON fd.id = rf.documento_id
WHERE ef.id IS NULL AND rf.id IS NULL AND fd.activo = 1;

-- Archivos más grandes
SELECT TOP 10 nombre_archivo, tamano_bytes/1024/1024 as size_mb, entidad_tipo
FROM files_documentos
WHERE activo = 1
ORDER BY tamano_bytes DESC;

-- Archivos sin checksum (integridad no verificable)
SELECT COUNT(*) FROM files_documentos WHERE checksum_sha256 IS NULL;
```

### **Tareas de Mantenimiento:**

1. **Limpieza de archivos huérfanos** (mensual)
2. **Validación de integridad** (semanal)
3. **Backup de archivos** (diario)
4. **Revisión de espacio en disco** (diario)
5. **Auditoría de accesos** (mensual)

---

## 🚀 Ejemplos de Código Frontend

### **Angular - Subir Foto de Evaluación**

```typescript
async subirFotoEvaluacion(detalleId: number, archivo: File): Promise<void> {
  const formData = new FormData();
  formData.append('Archivo', archivo);
  formData.append('EntidadTipo', 'Evaluacion');
  formData.append('EntidadId', detalleId.toString());
  formData.append('Descripcion', 'Foto de evaluación');
  formData.append('Categoria', 'FotoEvaluacion');
  
  const response = await this.http.post<FileResponseDto>(
    '/api/Files/upload',
    formData
  ).toPromise();
  
  console.log('Foto subida:', response.nombreArchivo);
  console.log('URL descarga:', response.urlDescarga);
}
```

### **Angular - Mostrar Fotos de Reparación**

```typescript
async obtenerFotosReparacion(reparacionId: number): Promise<FileResponseDto[]> {
  const params = new HttpParams()
    .set('EntidadTipo', 'Reparacion')
    .set('EntidadId', reparacionId.toString())
    .set('SoloImagenes', 'true');
  
  return this.http.get<FileResponseDto[]>(
    '/api/Files/entity',
    { params }
  ).toPromise();
}
```

```html
<!-- Galería de fotos -->
<div class="grid grid-cols-3 gap-4">
  <div *ngFor="let foto of fotos" class="relative">
    <img [src]="foto.urlDescarga" [alt]="foto.descripcion" class="w-full h-48 object-cover rounded" />
    <span class="absolute top-2 right-2 bg-blue-500 text-white px-2 py-1 rounded text-xs">
      {{ foto.tamanoFormateado }}
    </span>
  </div>
</div>
```

---

## ✅ Checklist de Implementación

### **Completado:**
- [x] Enum `TipoEntidadDocumento`
- [x] Enum `CategoriaDocumento`
- [x] DTOs de request/response
- [x] Interfaz `IFileStorageService`
- [x] Implementación `FileStorageService`
- [x] Controlador `FilesController`
- [x] Documentación completa

### **Pendiente:**
- [ ] Registrar servicios en `Program.cs`
- [ ] Ejecutar script de mejoras en `files_documentos`
- [ ] Actualizar `FilesDocumento.cs` con nuevos campos
- [ ] Actualizar configuración en `WriteContext.cs`
- [ ] Agregar configuración en `appsettings.json`
- [ ] Probar endpoints con Postman/Thunder Client
- [ ] Refactorizar `FotosEvaluacionService` para usar servicio genérico
- [ ] Implementar `ReparacionFotoService`
- [ ] Unit tests

---

## 📞 Soporte

Para dudas o problemas, contactar al equipo de desarrollo.

**Versión:** 1.0  
**Fecha:** Octubre 2025  
**Autor:** Equipo CABs Backend
