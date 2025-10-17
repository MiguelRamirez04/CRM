using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using back_cabs.CRM.contexts;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.models.Files;
using back_cabs.CRM.DTOs.shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.services.shared
{
    /// <summary>
    /// Servicio para gestionar fotos de evaluación con conversión a WebP.
    /// </summary>
    public class FotosEvaluacionService
    {
        private readonly ReadOnlyContext _readonlyContext;
        private readonly WriteContext _writeContext;
        private readonly ImageProcessingService _imageProcessing;
        private readonly ILogger<FotosEvaluacionService> _logger;
        private readonly string _uploadPath;
        private readonly int _maxFileSizeMB;
        private readonly int _webpQuality;
        private readonly int _maxImageWidth;
        private readonly int _maxImageHeight;

        public FotosEvaluacionService(
            ReadOnlyContext readOnlyContext, 
            WriteContext writeContext,
            ImageProcessingService imageProcessing,
            ILogger<FotosEvaluacionService> logger,
            IConfiguration configuration)
        {
            _readonlyContext = readOnlyContext ?? throw new ArgumentNullException(nameof(readOnlyContext));
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _imageProcessing = imageProcessing ?? throw new ArgumentNullException(nameof(imageProcessing));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Cargar configuración
            var uploadPathConfig = configuration["FileStorage:UploadPath"];
            _uploadPath = string.IsNullOrEmpty(uploadPathConfig)
                ? Path.Combine(Directory.GetCurrentDirectory(), "CRM", "uploads", "evaluaciones")
                : Path.Combine(Directory.GetCurrentDirectory(), uploadPathConfig, "evaluaciones");

            _maxFileSizeMB = int.TryParse(configuration["FileStorage:MaxFileSizeMB"], out var maxSize) ? maxSize : 10;
            _webpQuality = int.TryParse(configuration["FileStorage:WebPQuality"], out var quality) ? quality : 80;
            _maxImageWidth = int.TryParse(configuration["FileStorage:MaxImageWidth"], out var width) ? width : 1920;
            _maxImageHeight = int.TryParse(configuration["FileStorage:MaxImageHeight"], out var height) ? height : 1080;

            // Crear directorio si no existe
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
                _logger.LogInformation("Directorio de uploads creado: {Path}", _uploadPath);
            }
        }
        #region Operacion de lectrua (Queries)

        ///<summary>
        /// Obtiene todas las operacoines de la base de datos.
        ///<summary>
        /// Pregunta estos comentarios son necesarios?
        public async Task<List<EvaluacionFotoResponseDto>> GetAllFotosAsync()
        {
            return await _readonlyContext.EvaluacionesFotos
                .Select(foto => new EvaluacionFotoResponseDto
                {
                    Id = foto.Id,
                    DetalleId = foto.DetalleId,
                    DocumentoId = foto.DocumentoId,
                    Tipo = foto.Tipo,
                    Descripcion = foto.Descripcion,
                    CreadoEn = foto.CreadoEn,
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una foto de evaluación por su ID.
        /// </summary>
        public async Task<EvaluacionFotoResponseDto?> GetFotoByIdAsync(int id)
        {
            var foto = await _readonlyContext.EvaluacionesFotos.FindAsync(id);
            if (foto == null)
            {
                return null;
            }

            return new EvaluacionFotoResponseDto
            {
                Id = foto.Id,
                DetalleId = foto.DetalleId,
                DocumentoId = foto.DocumentoId,
                Tipo = foto.Tipo,
                Descripcion = foto.Descripcion,
                CreadoEn = foto.CreadoEn,
            };
        }

        #endregion

        #region Operaciones de escritura (Commands)

        /// <summary>
        /// Crea una nueva entrada de foto de evaluación con conversión a WebP.
        /// </summary>
        public async Task<EvaluacionFotoResponseDto> CreateFotoAsync(EvaluacionFotoRequestDto requestDto, int usuarioId)
        {
            _logger.LogInformation("Iniciando subida de foto para detalle de evaluación {DetalleId} por usuario {UsuarioId}", 
                requestDto.DetalleId, usuarioId);

            // 1. Validar que el detalle existe
            var detalleExists = await _readonlyContext.EvaluacionesDetalles.AnyAsync(d => d.Id == requestDto.DetalleId);
            if (!detalleExists)
            {
                _logger.LogWarning("Detalle de evaluación con ID {DetalleId} no encontrado", requestDto.DetalleId);
                throw new KeyNotFoundException($"Detalle de evaluación con ID {requestDto.DetalleId} no encontrado.");
            }

            // 2. Validar archivo
            if (requestDto.Archivo == null || requestDto.Archivo.Length == 0)
                throw new ArgumentException("El archivo es requerido y no puede estar vacío.", nameof(requestDto.Archivo));

            if (!_imageProcessing.IsValidImage(requestDto.Archivo))
                throw new ArgumentException("Solo se permiten imágenes (JPEG, PNG, WebP, GIF).", nameof(requestDto.Archivo));

            var maxFileSizeBytes = _maxFileSizeMB * 1024 * 1024;
            if (requestDto.Archivo.Length > maxFileSizeBytes)
                throw new ArgumentException($"El archivo no puede exceder {_maxFileSizeMB}MB.", nameof(requestDto.Archivo));

            // 3. Validar contenido
            using (var stream = requestDto.Archivo.OpenReadStream())
            {
                if (!await _imageProcessing.IsValidImageContentAsync(stream))
                    throw new ArgumentException("El archivo no contiene una imagen válida.", nameof(requestDto.Archivo));
            }

            var strategy = _writeContext.Database.CreateExecutionStrategy();

            var ejecucionResult = await strategy.ExecuteAsync(async () =>
            {

                using var transaction = await _writeContext.Database.BeginTransactionAsync();
                try
                {
                    // 4. Generar nombre único
                    var originalFileName = Path.GetFileNameWithoutExtension(requestDto.Archivo.FileName);
                    var sanitizedFileName = SanitizeFileName(originalFileName);
                    var webpFileName = $"{Guid.NewGuid()}_{sanitizedFileName}.webp";
                    var webpFilePath = Path.Combine(_uploadPath, webpFileName);

                    // 5. Convertir a WebP
                    long tamanoBytes;
                    int ancho, alto;
                    using (var inputStream = requestDto.Archivo.OpenReadStream())
                    {
                        (tamanoBytes, ancho, alto) = await _imageProcessing.ConvertToWebPAsync(
                            inputStream,
                            webpFilePath,
                            _webpQuality,
                            _maxImageWidth,
                            _maxImageHeight);
                    }

                    // 6. Crear metadatos
                    var metadatos = new
                    {
                        ArchivoOriginal = requestDto.Archivo.FileName,
                        TamanoOriginal = requestDto.Archivo.Length,
                        ContentTypeOriginal = requestDto.Archivo.ContentType,
                        Ancho = ancho,
                        Alto = alto,
                        CalidadWebP = _webpQuality,
                        FechaConversion = DateTime.UtcNow
                    };
                    var metadatosJson = JsonSerializer.Serialize(metadatos);

                    // 7. Crear registro en files_documentos
                    var documento = new FilesDocumento
                    {
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

                    // 8. Crear registro en evaluacion_fotos
                    var nuevaFoto = new EvaluacionFoto
                    {
                        DetalleId = requestDto.DetalleId,
                        DocumentoId = documento.Id,
                        Tipo = requestDto.Tipo,
                        Descripcion = requestDto.Descripcion,
                        CreadoEn = DateTime.UtcNow
                    };
                    _writeContext.EvaluacionesFotos.Add(nuevaFoto);
                    await _writeContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    _logger.LogInformation("Foto subida exitosamente para detalle {DetalleId}, Foto ID: {FotoId}, Tamaño: {TamanoKB}KB",
                        requestDto.DetalleId, nuevaFoto.Id, tamanoBytes / 1024);

                    return new EvaluacionFotoResponseDto
                    {
                        Id = nuevaFoto.Id,
                        DetalleId = nuevaFoto.DetalleId,
                        DocumentoId = nuevaFoto.DocumentoId,
                        Tipo = nuevaFoto.Tipo,
                        Descripcion = nuevaFoto.Descripcion,
                        CreadoEn = nuevaFoto.CreadoEn,
                        NombreArchivo = documento.NombreArchivo,
                        MimeType = documento.MimeType,
                        TamanoBytes = documento.TamanoBytes,
                        UrlDescarga = $"/api/FotosEvaluacion/{nuevaFoto.Id}/download"
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al subir foto para detalle {DetalleId}", requestDto.DetalleId);
                    throw new InvalidOperationException("Error al guardar la foto.", ex);
                }
            });
            return ejecucionResult;
        }

        /// <summary>
        /// Actualiza metadatos de una foto (NO se puede cambiar el archivo).
        /// </summary>
        public async Task<EvaluacionFoto?> UpdateFotoAsync(int id, string? tipo, string? descripcion)
        {
            var fotoExistente = await _writeContext.EvaluacionesFotos.FindAsync(id);

            if (fotoExistente == null)
            {
                return null;
            }

            fotoExistente.Tipo = tipo ?? fotoExistente.Tipo;
            fotoExistente.Descripcion = descripcion ?? fotoExistente.Descripcion;

            await _writeContext.SaveChangesAsync();
            _logger.LogInformation("Foto {FotoId} actualizada", id);
            return fotoExistente;
        }

        /// <summary>
        /// Elimina una foto de evaluación por su ID (archivo físico + BD).
        /// </summary>
        public async Task<bool> DeleteFotoAsync(int id)
        {
            _logger.LogInformation("Eliminando foto con ID {FotoId}", id);

            var fotoAEliminar = await _writeContext.EvaluacionesFotos
                .Include(f => f.Documento)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fotoAEliminar == null)
            {
                _logger.LogWarning("Foto con ID {FotoId} no encontrada", id);
                return false;
            }

            // Eliminar archivo físico
            if (fotoAEliminar.Documento != null && File.Exists(fotoAEliminar.Documento.RutaAlmacenamiento))
            {
                try
                {
                    File.Delete(fotoAEliminar.Documento.RutaAlmacenamiento);
                    _logger.LogInformation("Archivo físico eliminado: {Ruta}", fotoAEliminar.Documento.RutaAlmacenamiento);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al eliminar archivo físico: {Ruta}", fotoAEliminar.Documento.RutaAlmacenamiento);
                }
            }

            // Eliminar registros en BD
            if (fotoAEliminar.Documento != null)
            {
                _writeContext.Documentos.Remove(fotoAEliminar.Documento);
            }
            _writeContext.EvaluacionesFotos.Remove(fotoAEliminar);
            await _writeContext.SaveChangesAsync();

            _logger.LogInformation("Foto {FotoId} eliminada exitosamente", id);
            return true;
        }

        /// <summary>
        /// Descarga una foto por su ID.
        /// </summary>
        public async Task<(Stream fileStream, string contentType, string fileName)?> DownloadFotoAsync(int fotoId)
        {
            _logger.LogInformation("Descargando foto con ID {FotoId}", fotoId);

            var foto = await _readonlyContext.EvaluacionesFotos
                .Include(f => f.Documento)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == fotoId);

            if (foto?.Documento == null)
            {
                _logger.LogWarning("Foto con ID {FotoId} no encontrada en BD", fotoId);
                return null;
            }

            if (!File.Exists(foto.Documento.RutaAlmacenamiento))
            {
                _logger.LogError("Archivo físico no encontrado: {Ruta}", foto.Documento.RutaAlmacenamiento);
                return null;
            }

            var fileStream = File.OpenRead(foto.Documento.RutaAlmacenamiento);

            _logger.LogInformation("Foto {FotoId} descargada exitosamente", fotoId);
            return (fileStream, foto.Documento.MimeType ?? "image/webp", foto.Documento.NombreArchivo);
        }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// Sanitiza el nombre del archivo para evitar caracteres inválidos.
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrWhiteSpace(sanitized) ? "archivo" : sanitized;
        }

        #endregion
    }
}
