using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.models.Files;
using back_cabs.CRM.models.Soporte;
using back_cabs.CRM.services.shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.services.Soporte
{
    /// <summary>
    /// Servicio para gestionar fotos de reparaciones con conversión automática a WebP.
    /// Implementa subida, descarga, listado y eliminación de fotos.
    /// </summary>
    public class ReparacionFotoService
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ImageProcessingService _imageProcessing;
        private readonly ILogger<ReparacionFotoService> _logger;
        private readonly string _uploadPath;
        private readonly int _maxFileSizeMB;
        private readonly int _webpQuality;
        private readonly int _maxImageWidth;
        private readonly int _maxImageHeight;

        public ReparacionFotoService(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            ImageProcessingService imageProcessing,
            ILogger<ReparacionFotoService> logger,
            IConfiguration configuration)
        {
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _imageProcessing = imageProcessing ?? throw new ArgumentNullException(nameof(imageProcessing));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Cargar configuración
            var uploadPathConfig = configuration["FileStorage:UploadPath"];
            _uploadPath = string.IsNullOrEmpty(uploadPathConfig)
                ? Path.Combine(Directory.GetCurrentDirectory(), "CRM", "uploads", "reparaciones")
                : Path.Combine(Directory.GetCurrentDirectory(), uploadPathConfig, "reparaciones");

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

        /// <summary>
        /// Sube una foto, la convierte a WebP y la asocia a una reparación.
        /// </summary>
        /// <param name="reparacionId">ID de la reparación.</param>
        /// <param name="dto">DTO con el archivo y metadatos.</param>
        /// <param name="usuarioId">ID del usuario que sube la foto.</param>
        /// <returns>DTO con la información de la foto creada.</returns>
        /// <exception cref="KeyNotFoundException">Si la reparación no existe.</exception>
        /// <exception cref="ArgumentException">Si el archivo es inválido.</exception>
        /// <exception cref="InvalidOperationException">Si hay error al guardar.</exception>
        public async Task<ReparacionFotoResponseDto> UploadFotoAsync(
            int reparacionId, 
            ReparacionFotoUploadRequestDto dto, 
            int usuarioId)
        {
            _logger.LogInformation("Iniciando subida de foto para reparación {ReparacionId} por usuario {UsuarioId}", 
                reparacionId, usuarioId);

            // 1. Validar que la reparación existe
            var reparacionExists = await _readContext.Reparaciones.AnyAsync(r => r.Id == reparacionId);
            if (!reparacionExists)
            {
                _logger.LogWarning("Reparación con ID {ReparacionId} no encontrada", reparacionId);
                throw new KeyNotFoundException($"Reparación con ID {reparacionId} no encontrada.");
            }

            // 2. Validar archivo
            if (dto.Archivo == null || dto.Archivo.Length == 0)
                throw new ArgumentException("El archivo es requerido y no puede estar vacío.", nameof(dto.Archivo));

            if (!_imageProcessing.IsValidImage(dto.Archivo))
                throw new ArgumentException("Solo se permiten imágenes (JPEG, PNG, WebP, GIF).", nameof(dto.Archivo));

            var maxFileSizeBytes = _maxFileSizeMB * 1024 * 1024;
            if (dto.Archivo.Length > maxFileSizeBytes)
                throw new ArgumentException($"El archivo no puede exceder {_maxFileSizeMB}MB.", nameof(dto.Archivo));

            // 3. Validar contenido del archivo
            using (var stream = dto.Archivo.OpenReadStream())
            {
                if (!await _imageProcessing.IsValidImageContentAsync(stream))
                    throw new ArgumentException("El archivo no contiene una imagen válida.", nameof(dto.Archivo));
            }

            using var transaction = await _writeContext.Database.BeginTransactionAsync();
            try
            {
                // 4. Generar nombre único con extensión .webp
                var originalFileName = Path.GetFileNameWithoutExtension(dto.Archivo.FileName);
                var sanitizedFileName = SanitizeFileName(originalFileName);
                var webpFileName = $"{Guid.NewGuid()}_{sanitizedFileName}.webp";
                var webpFilePath = Path.Combine(_uploadPath, webpFileName);

                // 5. Convertir a WebP
                long tamanoBytes;
                int ancho, alto;
                using (var inputStream = dto.Archivo.OpenReadStream())
                {
                    (tamanoBytes, ancho, alto) = await _imageProcessing.ConvertToWebPAsync(
                        inputStream, 
                        webpFilePath, 
                        _webpQuality,
                        _maxImageWidth,
                        _maxImageHeight);
                }

                // 6. Crear metadatos JSON
                var metadatos = new
                {
                    ArchivoOriginal = dto.Archivo.FileName,
                    TamanoOriginal = dto.Archivo.Length,
                    ContentTypeOriginal = dto.Archivo.ContentType,
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
                    EntidadTipo = "Reparacion",
                    EntidadId = reparacionId,
                    NombreArchivo = webpFileName,
                    RutaAlmacenamiento = webpFilePath,
                    MimeType = "image/webp",
                    TamanoBytes = tamanoBytes,
                    MetadatosJson = metadatosJson
                };
                _writeContext.Documentos.Add(documento);
                await _writeContext.SaveChangesAsync();

                // 8. Crear registro en reparacion_fotos
                var reparacionFoto = new ReparacionFoto
                {
                    ReparacionId = reparacionId,
                    DocumentoId = documento.Id,
                    Etapa = dto.Etapa,
                    Descripcion = dto.Descripcion,
                    CreadoEn = DateTime.UtcNow
                };
                _writeContext.Set<ReparacionFoto>().Add(reparacionFoto);
                await _writeContext.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Foto subida exitosamente para reparación {ReparacionId}, Foto ID: {FotoId}, Tamaño: {TamanoKB}KB", 
                    reparacionId, reparacionFoto.Id, tamanoBytes / 1024);

                return await MapToResponseDtoAsync(reparacionFoto, documento);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al subir foto para reparación {ReparacionId}", reparacionId);
                throw new InvalidOperationException("Error al guardar la foto.", ex);
            }
        }

        /// <summary>
        /// Obtiene todas las fotos de una reparación.
        /// </summary>
        /// <param name="reparacionId">ID de la reparación.</param>
        /// <returns>Lista de DTOs con las fotos.</returns>
        public async Task<List<ReparacionFotoResponseDto>> GetFotosByReparacionIdAsync(int reparacionId)
        {
            _logger.LogInformation("Consultando fotos de reparación {ReparacionId}", reparacionId);

            var fotos = await _readContext.Set<ReparacionFoto>()
                .Include(f => f.Documento)
                .ThenInclude(d => d.CreadoPorUsuario)
                .Where(f => f.ReparacionId == reparacionId)
                .OrderBy(f => f.CreadoEn)
                .AsNoTracking()
                .ToListAsync();

            var result = new List<ReparacionFotoResponseDto>();
            foreach (var foto in fotos)
            {
                result.Add(await MapToResponseDtoAsync(foto, foto.Documento));
            }

            _logger.LogInformation("Se encontraron {Count} fotos para reparación {ReparacionId}", result.Count, reparacionId);
            return result;
        }

        /// <summary>
        /// Descarga una foto por su ID.
        /// </summary>
        /// <param name="fotoId">ID de la foto.</param>
        /// <returns>Tupla con stream, contentType y fileName, o null si no existe.</returns>
        public async Task<(Stream fileStream, string contentType, string fileName)?> DownloadFotoAsync(int fotoId)
        {
            _logger.LogInformation("Descargando foto con ID {FotoId}", fotoId);

            var foto = await _readContext.Set<ReparacionFoto>()
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

            var fileStream = new FileStream(
                foto.Documento.RutaAlmacenamiento, 
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.Read);

            _logger.LogInformation("Foto {FotoId} descargada exitosamente", fotoId);
            return (fileStream, foto.Documento.MimeType ?? "image/webp", foto.Documento.NombreArchivo);
        }

        /// <summary>
        /// Elimina una foto y su archivo físico.
        /// </summary>
        /// <param name="fotoId">ID de la foto a eliminar.</param>
        /// <exception cref="KeyNotFoundException">Si la foto no existe.</exception>
        public async Task DeleteFotoAsync(int fotoId)
        {
            _logger.LogInformation("Eliminando foto con ID {FotoId}", fotoId);

            var foto = await _writeContext.Set<ReparacionFoto>()
                .Include(f => f.Documento)
                .FirstOrDefaultAsync(f => f.Id == fotoId);

            if (foto == null)
            {
                _logger.LogWarning("Foto con ID {FotoId} no encontrada", fotoId);
                throw new KeyNotFoundException($"Foto con ID {fotoId} no encontrada.");
            }

            // Eliminar archivo físico
            if (File.Exists(foto.Documento.RutaAlmacenamiento))
            {
                try
                {
                    File.Delete(foto.Documento.RutaAlmacenamiento);
                    _logger.LogInformation("Archivo físico eliminado: {Ruta}", foto.Documento.RutaAlmacenamiento);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al eliminar archivo físico: {Ruta}", foto.Documento.RutaAlmacenamiento);
                }
            }

            // Eliminar registros en BD (cascade debe eliminar ReparacionFoto automáticamente)
            _writeContext.Documentos.Remove(foto.Documento);
            await _writeContext.SaveChangesAsync();

            _logger.LogInformation("Foto {FotoId} eliminada exitosamente", fotoId);
        }

        /// <summary>
        /// Mapea una entidad ReparacionFoto a DTO de respuesta.
        /// </summary>
        private async Task<ReparacionFotoResponseDto> MapToResponseDtoAsync(ReparacionFoto foto, FilesDocumento documento)
        {
            var usuario = await _readContext.UsuariosAuth
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == documento.CreadoPorUsuarioId);
            
            return new ReparacionFotoResponseDto
            {
                Id = foto.Id,
                ReparacionId = foto.ReparacionId,
                DocumentoId = foto.DocumentoId,
                Etapa = foto.Etapa,
                Descripcion = foto.Descripcion,
                CreadoEn = foto.CreadoEn,
                NombreArchivo = documento.NombreArchivo,
                MimeType = documento.MimeType,
                TamanoBytes = documento.TamanoBytes,
                CreadoPorUsuario = usuario != null ? $"{usuario.Nombre} {usuario.Apellido}".Trim() : "Desconocido",
                UrlDescarga = $"/api/reparaciones/{foto.ReparacionId}/fotos/{foto.Id}/download",
                Metadatos = documento.MetadatosJson
            };
        }

        /// <summary>
        /// Sanitiza el nombre del archivo para evitar caracteres inválidos.
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrWhiteSpace(sanitized) ? "archivo" : sanitized;
        }
    }
}
