# 📋 PLAN DE INTEGRACIÓN: Sistema de Archivos con Tablas de Fotos

## 🎯 OBJETIVO GENERAL
Refactorizar los servicios `FotosEvaluacionService` y `ReparacionFotoService` para utilizar el sistema centralizado `FileStorageService`, eliminando duplicación de código y consolidando la lógica de manejo de archivos.

---

## 📊 ANÁLISIS DE ESTADO ACTUAL

### **Servicios Existentes con Lógica Duplicada:**

#### 1. **FotosEvaluacionService**
- ✅ **Funcionalidad:** Sube fotos de evaluación y las convierte a WebP
- ⚠️ **Problema:** Duplica lógica de conversión WebP, validación, y almacenamiento
- 📁 **Tablas:** `evaluacion_fotos` → `files_documentos`
- 🔗 **Relación:** `EvaluacionFoto.DocumentoId` → `FilesDocumento.Id`

#### 2. **ReparacionFotoService**
- ✅ **Funcionalidad:** Sube fotos de reparación y las convierte a WebP
- ⚠️ **Problema:** Duplica lógica de conversión WebP, validación, y almacenamiento
- 📁 **Tablas:** `reparacion_fotos` → `files_documentos`
- 🔗 **Relación:** `ReparacionFoto.DocumentoId` → `FilesDocumento.Id`

#### 3. **FileStorageService** ✨ (Nuevo - Ya Implementado)
- ✅ **Funcionalidad:** Sistema centralizado de almacenamiento de archivos
- ✅ **Características:**
  - Conversión automática a WebP para `Evaluacion` y `Reparacion`
  - Validación de archivos (tamaño, tipo MIME, extensión)
  - Detección de duplicados por checksum SHA256
  - Soft delete con flag `activo`
  - Metadatos JSON estructurados
  - Descarga, listado y validación de integridad

---

## 🏗️ ARQUITECTURA PROPUESTA

### **Patrón de Diseño: Strategy + Facade**

```
┌─────────────────────────────────────────────────────────┐
│              CAPA DE CONTROLLERS                        │
│  - FotosEvaluacionController                            │
│  - ReparacionFotosController                            │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│         CAPA DE SERVICIOS ESPECIALIZADOS                │
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │  FotosEvaluacionService (FACADE)                │   │
│  │  - Lógica de negocio específica de evaluación  │   │
│  │  - Validaciones de detalle de evaluación       │   │
│  │  - Creación de registro en evaluacion_fotos    │   │
│  └──────────────────┬──────────────────────────────┘   │
│                     │                                   │
│                     │  Delega almacenamiento            │
│                     │                                   │
│  ┌─────────────────▼──────────────────────────────┐   │
│  │  ReparacionFotoService (FACADE)               │   │
│  │  - Lógica de negocio específica de reparación │   │
│  │  - Validaciones de reparación                 │   │
│  │  - Creación de registro en reparacion_fotos   │   │
│  └──────────────────┬──────────────────────────────┘   │
│                     │                                   │
└─────────────────────┼───────────────────────────────────┘
                      │
                      │  Ambos usan
                      │
                      ▼
┌─────────────────────────────────────────────────────────┐
│      SERVICIO CENTRALIZADO (CORE)                       │
│                                                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │  FileStorageService                              │  │
│  │  - Validación de archivos                        │  │
│  │  - Conversión a WebP (ImageProcessingService)    │  │
│  │  - Cálculo de checksum SHA256                    │  │
│  │  - Detección de duplicados                       │  │
│  │  - Almacenamiento físico                         │  │
│  │  - Creación de registro en files_documentos      │  │
│  │  - Descarga, eliminación, validación             │  │
│  └──────────────────────────────────────────────────┘  │
│                                                         │
└─────────────────────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────┐
│              CAPA DE DATOS                              │
│  - files_documentos (tabla central)                     │
│  - evaluacion_fotos (tabla de relación)                 │
│  - reparacion_fotos (tabla de relación)                 │
└─────────────────────────────────────────────────────────┘
```

---

## 📝 FLUJO DE DATOS PROPUESTO

### **Ejemplo: Subir Foto de Evaluación**

```
1. Controller recibe request:
   POST /api/FotosEvaluacion
   {
     "detalleId": 123,
     "archivo": <IFormFile>,
     "tipo": "FotoAntes",
     "descripcion": "Daño en puerta delantera"
   }

2. FotosEvaluacionService (Facade):
   ✅ Valida que el detalle_id existe en evaluaciones_detalles
   ✅ Extrae usuarioId de JWT claims
   
3. Delega a FileStorageService:
   var documento = await _fileStorageService.UploadFileAsync(
       file: request.Archivo,
       entidadTipo: TipoEntidadDocumento.Evaluacion,
       entidadId: request.DetalleId,
       usuarioId: usuarioId,
       descripcion: request.Descripcion,
       categoria: request.Tipo
   );
   
   🔧 FileStorageService hace:
      - Validación de archivo (tamaño, tipo MIME, extensión)
      - Cálculo de checksum SHA256
      - Detección de duplicados
      - Conversión a WebP (porque es Evaluacion)
      - Guarda en: CRM/uploads/evaluacion/{GUID}.webp
      - Crea registro en files_documentos

4. FotosEvaluacionService crea relación:
   var evaluacionFoto = new EvaluacionFoto {
       DetalleId = request.DetalleId,
       DocumentoId = documento.Id,  // ← FK al documento creado
       Tipo = request.Tipo,
       Descripcion = request.Descripcion,
       CreadoEn = DateTime.UtcNow
   };
   _writeContext.EvaluacionesFotos.Add(evaluacionFoto);
   await _writeContext.SaveChangesAsync();

5. Retorna respuesta unificada:
   {
     "id": 456,
     "detalleId": 123,
     "documentoId": 789,
     "tipo": "FotoAntes",
     "descripcion": "Daño en puerta delantera",
     "nombreArchivo": "a1b2c3d4-foto.webp",
     "mimeType": "image/webp",
     "tamanoBytes": 45632,
     "urlDescarga": "/api/Files/789/download",
     "creadoEn": "2025-10-18T10:30:00Z"
   }
```

---

## 🔄 PLAN DE DESARROLLO DETALLADO

### **FASE 1: Preparación y Análisis** ⏱️ 1-2 horas

#### **Tarea 1.1: Auditoría de Código Existente**
- [ ] Revisar todos los métodos de `FotosEvaluacionService`
- [ ] Revisar todos los métodos de `ReparacionFotoService`
- [ ] Identificar lógica común vs específica
- [ ] Documentar diferencias entre ambos servicios

#### **Tarea 1.2: Validar Integridad de Base de Datos**
- [ ] Verificar que todas las fotos actuales tienen `documento_id` válido
- [ ] Verificar consistencia entre `files_documentos` y tablas de fotos
- [ ] Ejecutar queries de validación:
```sql
-- Verificar fotos huérfanas en evaluacion_fotos
SELECT * FROM evaluacion_fotos ef
LEFT JOIN files_documentos fd ON ef.documento_id = fd.id
WHERE fd.id IS NULL;

-- Verificar fotos huérfanas en reparacion_fotos
SELECT * FROM reparacion_fotos rf
LEFT JOIN files_documentos fd ON rf.documento_id = fd.id
WHERE fd.id IS NULL;
```

#### **Tarea 1.3: Backup de Seguridad**
- [ ] Crear respaldo de tablas:
  - `files_documentos`
  - `evaluacion_fotos`
  - `reparacion_fotos`
- [ ] Documentar estado actual del directorio `CRM/uploads/`

---

### **FASE 2: Refactorización de FotosEvaluacionService** ⏱️ 3-4 horas

#### **Tarea 2.1: Actualizar Constructor**
```csharp
public class FotosEvaluacionService
{
    private readonly ReadOnlyContext _readContext;
    private readonly WriteContext _writeContext;
    private readonly IFileStorageService _fileStorageService; // ← Nuevo
    private readonly ILogger<FotosEvaluacionService> _logger;

    public FotosEvaluacionService(
        ReadOnlyContext readContext,
        WriteContext writeContext,
        IFileStorageService fileStorageService, // ← Inyectar servicio
        ILogger<FotosEvaluacionService> logger)
    {
        _readContext = readContext;
        _writeContext = writeContext;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }
}
```

#### **Tarea 2.2: Refactorizar Método `CreateFotoAsync`**

**❌ Código Actual (Duplicado):**
```csharp
public async Task<EvaluacionFotoResponseDto> CreateFotoAsync(
    EvaluacionFotoRequestDto requestDto, 
    int usuarioId)
{
    // 100+ líneas de código duplicado:
    // - Validación de archivo
    // - Conversión a WebP
    // - Generación de nombre único
    // - Cálculo de metadatos
    // - Guardado físico
    // - Creación en files_documentos
    // - Creación en evaluacion_fotos
}
```

**✅ Código Refactorizado (DRY):**
```csharp
public async Task<EvaluacionFotoResponseDto> CreateFotoAsync(
    EvaluacionFotoRequestDto requestDto, 
    int usuarioId)
{
    _logger.LogInformation(
        "Iniciando subida de foto para detalle {DetalleId}", 
        requestDto.DetalleId);

    // 1. Validación específica del dominio
    var detalleExists = await _readContext.EvaluacionesDetalles
        .AnyAsync(d => d.Id == requestDto.DetalleId);
    
    if (!detalleExists)
    {
        throw new KeyNotFoundException(
            $"Detalle de evaluación {requestDto.DetalleId} no encontrado.");
    }

    // 2. Delegar almacenamiento al servicio centralizado
    var documento = await _fileStorageService.UploadFileAsync(
        file: requestDto.Archivo,
        entidadTipo: TipoEntidadDocumento.Evaluacion,
        entidadId: requestDto.DetalleId,
        usuarioId: usuarioId,
        descripcion: requestDto.Descripcion,
        categoria: requestDto.Tipo
    );

    // 3. Crear relación en tabla específica
    var evaluacionFoto = new EvaluacionFoto
    {
        DetalleId = requestDto.DetalleId,
        DocumentoId = documento.Id,
        Tipo = requestDto.Tipo,
        Descripcion = requestDto.Descripcion,
        CreadoEn = DateTime.UtcNow
    };

    _writeContext.EvaluacionesFotos.Add(evaluacionFoto);
    await _writeContext.SaveChangesAsync();

    _logger.LogInformation(
        "Foto {FotoId} creada para detalle {DetalleId}, documento {DocumentoId}",
        evaluacionFoto.Id, requestDto.DetalleId, documento.Id);

    // 4. Mapear respuesta
    return new EvaluacionFotoResponseDto
    {
        Id = evaluacionFoto.Id,
        DetalleId = evaluacionFoto.DetalleId,
        DocumentoId = evaluacionFoto.DocumentoId,
        Tipo = evaluacionFoto.Tipo,
        Descripcion = evaluacionFoto.Descripcion,
        NombreArchivo = documento.NombreArchivo,
        MimeType = documento.MimeType,
        TamanoBytes = documento.TamanoBytes,
        UrlDescarga = $"/api/Files/{documento.Id}/download",
        CreadoEn = evaluacionFoto.CreadoEn
    };
}
```

**📊 Reducción de Código: 150 líneas → 50 líneas (67% menos)**

#### **Tarea 2.3: Refactorizar Método `GetFotosByDetalleIdAsync`**
```csharp
public async Task<List<EvaluacionFotoResponseDto>> GetFotosByDetalleIdAsync(
    int detalleId)
{
    var fotos = await _readContext.EvaluacionesFotos
        .Where(f => f.DetalleId == detalleId)
        .Include(f => f.Documento) // ← Incluir relación
        .OrderByDescending(f => f.CreadoEn)
        .ToListAsync();

    return fotos.Select(f => new EvaluacionFotoResponseDto
    {
        Id = f.Id,
        DetalleId = f.DetalleId,
        DocumentoId = f.DocumentoId,
        Tipo = f.Tipo,
        Descripcion = f.Descripcion,
        NombreArchivo = f.Documento?.NombreArchivo,
        MimeType = f.Documento?.MimeType,
        TamanoBytes = f.Documento?.TamanoBytes,
        UrlDescarga = $"/api/Files/{f.DocumentoId}/download",
        CreadoEn = f.CreadoEn
    }).ToList();
}
```

#### **Tarea 2.4: Refactorizar Método `DownloadFotoAsync`**

**✅ Delegar completamente a FileStorageService:**
```csharp
public async Task<(Stream stream, string contentType, string fileName)?> 
    DownloadFotoAsync(int fotoId)
{
    // 1. Obtener foto y validar existencia
    var foto = await _readContext.EvaluacionesFotos
        .AsNoTracking()
        .FirstOrDefaultAsync(f => f.Id == fotoId);

    if (foto == null)
    {
        _logger.LogWarning("Foto de evaluación {FotoId} no encontrada", fotoId);
        return null;
    }

    // 2. Delegar descarga al servicio centralizado
    return await _fileStorageService.DownloadFileAsync(foto.DocumentoId);
}
```

#### **Tarea 2.5: Refactorizar Método `DeleteFotoAsync`**
```csharp
public async Task<bool> DeleteFotoAsync(int fotoId, int usuarioId)
{
    // 1. Obtener foto
    var foto = await _writeContext.EvaluacionesFotos.FindAsync(fotoId);
    if (foto == null)
    {
        return false;
    }

    // 2. Soft delete del documento (preserva integridad referencial)
    var deleted = await _fileStorageService.DeleteFileAsync(
        foto.DocumentoId, 
        usuarioId);

    if (!deleted)
    {
        _logger.LogError(
            "No se pudo eliminar documento {DocumentoId} para foto {FotoId}",
            foto.DocumentoId, fotoId);
        return false;
    }

    // 3. Eliminar registro de relación (opcional, según reglas de negocio)
    _writeContext.EvaluacionesFotos.Remove(foto);
    await _writeContext.SaveChangesAsync();

    _logger.LogInformation(
        "Foto {FotoId} y documento {DocumentoId} eliminados",
        fotoId, foto.DocumentoId);

    return true;
}
```

#### **Tarea 2.6: Eliminar Código Obsoleto**
- [ ] Eliminar métodos privados duplicados:
  - `SanitizeFileName()` → Ya existe en FileStorageService
  - Validaciones de archivo → Delegadas a FileStorageService
  - Lógica de conversión WebP → Delegada a FileStorageService
- [ ] Eliminar propiedades de configuración redundantes
- [ ] Eliminar inyección de `ImageProcessingService` (ya no se usa directamente)

---

### **FASE 3: Refactorización de ReparacionFotoService** ⏱️ 3-4 horas

#### **Tarea 3.1: Aplicar Mismos Cambios que FotosEvaluacionService**
- [ ] Actualizar constructor con `IFileStorageService`
- [ ] Refactorizar `UploadFotoAsync()`
- [ ] Refactorizar `GetFotosByReparacionIdAsync()`
- [ ] Refactorizar `DownloadFotoAsync()`
- [ ] Refactorizar `DeleteFotoAsync()`
- [ ] Eliminar código duplicado

**💡 Nota:** El patrón es idéntico, solo cambia:
- Entidad: `TipoEntidadDocumento.Reparacion`
- Tabla de relación: `reparacion_fotos`
- Validación: verificar que `reparacion_id` existe

---

### **FASE 4: Actualizar Controllers** ⏱️ 1-2 horas

#### **Tarea 4.1: Actualizar FotosEvaluacionController**
```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> UploadFoto(
    [FromForm] EvaluacionFotoRequestDto request)
{
    try
    {
        var usuarioId = int.Parse(User.FindFirst("id")?.Value ?? "0");
        
        var resultado = await _fotosEvaluacionService.CreateFotoAsync(
            request, 
            usuarioId);

        return CreatedAtAction(
            nameof(GetFotoById), 
            new { id = resultado.Id }, 
            resultado);
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(ex.Message);
    }
    catch (ArgumentException ex)
    {
        return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al subir foto de evaluación");
        return StatusCode(500, "Error interno del servidor");
    }
}

[HttpGet("{id}/download")]
[Authorize]
public async Task<IActionResult> DownloadFoto(int id)
{
    var resultado = await _fotosEvaluacionService.DownloadFotoAsync(id);
    
    if (resultado == null)
    {
        return NotFound($"Foto {id} no encontrada");
    }

    return File(resultado.Value.stream, resultado.Value.contentType, 
        resultado.Value.fileName);
}
```

#### **Tarea 4.2: Actualizar ReparacionFotosController**
- [ ] Aplicar misma estructura que FotosEvaluacionController
- [ ] Mantener consistencia en manejo de errores
- [ ] Agregar logging adecuado

---

### **FASE 5: Testing y Validación** ⏱️ 2-3 horas

#### **Tarea 5.1: Pruebas Unitarias**
```csharp
public class FotosEvaluacionServiceTests
{
    [Fact]
    public async Task CreateFotoAsync_ConDetalleValido_CreaFotoYDocumento()
    {
        // Arrange
        var mockFileStorage = new Mock<IFileStorageService>();
        mockFileStorage
            .Setup(x => x.UploadFileAsync(
                It.IsAny<IFormFile>(),
                TipoEntidadDocumento.Evaluacion,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new FilesDocumento { Id = 123 });

        var service = new FotosEvaluacionService(
            _readContext, 
            _writeContext, 
            mockFileStorage.Object, 
            _logger);

        var request = new EvaluacionFotoRequestDto
        {
            DetalleId = 1,
            Archivo = CreateMockFile(),
            Tipo = "FotoAntes",
            Descripcion = "Test"
        };

        // Act
        var resultado = await service.CreateFotoAsync(request, 999);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(123, resultado.DocumentoId);
        mockFileStorage.Verify(x => x.UploadFileAsync(
            It.IsAny<IFormFile>(),
            TipoEntidadDocumento.Evaluacion,
            1,
            999,
            "Test",
            "FotoAntes"), Times.Once);
    }

    [Fact]
    public async Task CreateFotoAsync_ConDetalleInexistente_LanzaException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateFotoAsync(request, 999));
    }
}
```

#### **Tarea 5.2: Pruebas de Integración**
- [ ] Subir foto de evaluación y verificar:
  - Registro en `files_documentos`
  - Registro en `evaluacion_fotos`
  - Archivo físico existe en `CRM/uploads/evaluacion/`
  - Formato WebP correcto
  - Metadatos JSON válidos

- [ ] Subir foto de reparación y verificar lo mismo

- [ ] Probar descarga y verificar:
  - Stream correcto
  - Content-Type correcto
  - Nombre de archivo original preservado

- [ ] Probar eliminación y verificar:
  - Soft delete en `files_documentos` (activo = false)
  - Registro eliminado en tabla de fotos
  - Archivo físico permanece (para auditoría)

#### **Tarea 5.3: Pruebas en Swagger**
```http
### 1. Subir foto de evaluación
POST https://localhost:5176/api/FotosEvaluacion
Content-Type: multipart/form-data
Authorization: Bearer {{token}}

detalleId: 1
archivo: @foto_test.jpg
tipo: FotoAntes
descripcion: Daño en puerta izquierda

### 2. Listar fotos de detalle
GET https://localhost:5176/api/FotosEvaluacion/detalle/1
Authorization: Bearer {{token}}

### 3. Descargar foto
GET https://localhost:5176/api/FotosEvaluacion/456/download
Authorization: Bearer {{token}}

### 4. Eliminar foto
DELETE https://localhost:5176/api/FotosEvaluacion/456
Authorization: Bearer {{token}}
```

#### **Tarea 5.4: Validación de Regresión**
- [ ] Verificar que todas las fotos existentes siguen siendo accesibles
- [ ] Verificar que no hay pérdida de datos
- [ ] Verificar que los endpoints legacy siguen funcionando

---

### **FASE 6: Optimizaciones y Mejoras** ⏱️ 2-3 horas

#### **Tarea 6.1: Optimizar Consultas**
```csharp
// Agregar índices en base de datos
CREATE INDEX IX_evaluacion_fotos_detalle_id 
ON evaluacion_fotos(detalle_id) 
INCLUDE (documento_id, tipo, creado_en);

CREATE INDEX IX_reparacion_fotos_reparacion_id 
ON reparacion_fotos(reparacion_id) 
INCLUDE (documento_id, etapa, creado_en);
```

#### **Tarea 6.2: Agregar Cache**
```csharp
public class FotosEvaluacionService
{
    private readonly IMemoryCache _cache;

    public async Task<List<EvaluacionFotoResponseDto>> 
        GetFotosByDetalleIdAsync(int detalleId)
    {
        var cacheKey = $"evaluacion_fotos_{detalleId}";
        
        if (_cache.TryGetValue(cacheKey, out List<EvaluacionFotoResponseDto>? cached))
        {
            return cached!;
        }

        var fotos = await _readContext.EvaluacionesFotos
            .Where(f => f.DetalleId == detalleId)
            .Include(f => f.Documento)
            .ToListAsync();

        var resultado = MapToDto(fotos);
        
        _cache.Set(cacheKey, resultado, TimeSpan.FromMinutes(5));
        
        return resultado;
    }
}
```

#### **Tarea 6.3: Agregar Validaciones de Negocio**
```csharp
// Validar que no se suban más de X fotos por detalle
public async Task<EvaluacionFotoResponseDto> CreateFotoAsync(
    EvaluacionFotoRequestDto request, int usuarioId)
{
    // Contar fotos existentes
    var countFotos = await _readContext.EvaluacionesFotos
        .CountAsync(f => f.DetalleId == request.DetalleId);

    if (countFotos >= 10) // Límite de negocio
    {
        throw new InvalidOperationException(
            "Se ha alcanzado el límite de 10 fotos por detalle.");
    }

    // ... resto del código
}
```

#### **Tarea 6.4: Agregar Eventos de Dominio**
```csharp
// Notificar cuando se suba una foto
public class FotoSubidaEvent
{
    public int DocumentoId { get; set; }
    public TipoEntidadDocumento EntidadTipo { get; set; }
    public int EntidadId { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaSubida { get; set; }
}

// En el servicio
await _eventBus.PublishAsync(new FotoSubidaEvent
{
    DocumentoId = documento.Id,
    EntidadTipo = TipoEntidadDocumento.Evaluacion,
    EntidadId = request.DetalleId,
    UsuarioId = usuarioId,
    FechaSubida = DateTime.UtcNow
});
```

---

### **FASE 7: Documentación** ⏱️ 1-2 horas

#### **Tarea 7.1: Actualizar XML Documentation**
- [ ] Documentar todos los métodos públicos
- [ ] Documentar parámetros y retornos
- [ ] Documentar excepciones lanzadas

#### **Tarea 7.2: Crear Diagramas de Flujo**
- [ ] Diagrama de secuencia para subida de foto
- [ ] Diagrama de clases actualizado
- [ ] Diagrama de base de datos con relaciones

#### **Tarea 7.3: Actualizar README**
- [ ] Documentar nuevos endpoints
- [ ] Documentar cambios en arquitectura
- [ ] Agregar ejemplos de uso

---

## 📊 MÉTRICAS DE ÉXITO

### **Código**
- ✅ Reducción de duplicación: >60%
- ✅ Líneas de código eliminadas: ~300 líneas
- ✅ Cobertura de pruebas: >80%
- ✅ Complejidad ciclomática: <10 por método

### **Performance**
- ✅ Tiempo de subida: <2 segundos (archivo 5MB)
- ✅ Tiempo de descarga: <1 segundo
- ✅ Consultas a BD: Optimizadas con Include()
- ✅ Cache hit rate: >70%

### **Calidad**
- ✅ Separación de responsabilidades (SRP)
- ✅ Principio DRY aplicado
- ✅ Inyección de dependencias correcta
- ✅ Manejo de errores robusto
- ✅ Logging adecuado

---

## 🚨 RIESGOS Y MITIGACIONES

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|------------|
| Pérdida de datos durante migración | Baja | Alto | Backup completo antes de empezar |
| Incompatibilidad con código existente | Media | Medio | Mantener compatibilidad backwards durante transición |
| Degradación de performance | Baja | Medio | Tests de carga antes y después |
| Errores en validaciones | Media | Alto | Tests exhaustivos de casos límite |

---

## 🔄 ROLLBACK PLAN

Si algo sale mal durante la implementación:

1. **Restaurar backup de base de datos**
2. **Revertir cambios en Git:**
   ```bash
   git revert HEAD~1
   git push origin controlv2
   ```
3. **Restaurar directorio de uploads**
4. **Redeployar versión anterior**

---

## 📅 CRONOGRAMA ESTIMADO

| Fase | Duración | Responsable | Estado |
|------|----------|-------------|--------|
| 1. Preparación | 1-2 horas | Dev | ⏳ Pendiente |
| 2. Refactor FotosEvaluacion | 3-4 horas | Dev | ⏳ Pendiente |
| 3. Refactor ReparacionFoto | 3-4 horas | Dev | ⏳ Pendiente |
| 4. Actualizar Controllers | 1-2 horas | Dev | ⏳ Pendiente |
| 5. Testing | 2-3 horas | QA/Dev | ⏳ Pendiente |
| 6. Optimizaciones | 2-3 horas | Dev | ⏳ Pendiente |
| 7. Documentación | 1-2 horas | Dev | ⏳ Pendiente |

**TOTAL: 13-20 horas (2-3 días de trabajo)**

---

## ✅ CHECKLIST FINAL

### **Antes de Empezar:**
- [ ] Backup de base de datos realizado
- [ ] Backup de directorio uploads realizado
- [ ] Rama de desarrollo creada (`feature/integrate-file-storage`)
- [ ] Equipo notificado del cambio

### **Durante Desarrollo:**
- [ ] Tests unitarios escritos y pasando
- [ ] Tests de integración ejecutados
- [ ] Code review completado
- [ ] Swagger actualizado y probado

### **Antes de Merge:**
- [ ] Todas las pruebas pasan (verde)
- [ ] Documentación actualizada
- [ ] Performance validada
- [ ] Aprobación de al menos 1 reviewer

### **Después de Deploy:**
- [ ] Monitorear logs por 24 horas
- [ ] Verificar métricas de performance
- [ ] Validar que no hay errores en producción
- [ ] Actualizar wiki/documentación interna

---

## 📚 REFERENCIAS

- [Patrón Facade](https://refactoring.guru/design-patterns/facade)
- [Principios SOLID](https://www.digitalocean.com/community/conceptual_articles/s-o-l-i-d-the-first-five-principles-of-object-oriented-design)
- [ASP.NET Core File Upload Best Practices](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads)
- [Entity Framework Core Performance](https://learn.microsoft.com/en-us/ef/core/performance/)

---

## 🎯 RESUMEN EJECUTIVO

**Objetivo:** Eliminar duplicación de código y centralizar lógica de manejo de archivos.

**Enfoque:** Patrón Facade - Los servicios específicos (`FotosEvaluacionService`, `ReparacionFotoService`) se convierten en fachadas que validan reglas de negocio y delegan el almacenamiento físico a `FileStorageService`.

**Beneficios:**
- ✅ 60%+ menos código duplicado
- ✅ Mantenimiento más fácil (un solo lugar para cambios)
- ✅ Mayor testabilidad (servicios desacoplados)
- ✅ Mejor escalabilidad (agregar nuevos tipos de fotos es trivial)
- ✅ Arquitectura limpia (separación de responsabilidades)

**Riesgo:** Bajo (con backups y tests adecuados)

**Retorno de Inversión:** Alto (ahorro de tiempo en mantenimiento futuro)

---

**Creado:** 2025-10-18  
**Versión:** 1.0  
**Autor:** GitHub Copilot  
**Estado:** 📋 Planificación Completa - Listo para Implementación
