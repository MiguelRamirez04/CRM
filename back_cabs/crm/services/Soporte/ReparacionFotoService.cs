using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.models.Files;
using back_cabs.CRM.models.Soporte;
using back_cabs.CRM.services.shared;
using back_cabs.CRM.Interfaces.Soporte; // <-- Inyectar la Interfaz
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace back_cabs.CRM.services.Soporte
{
    public class ReparacionFotoService
    {
        // Dependencias limpias: Repositorio (datos), Procesador (imágenes), Logger y Config
        private readonly IReparacionFotoRepository _repository;
        private readonly ImageProcessingService _imageProcessing;
        private readonly ILogger<ReparacionFotoService> _logger;
        
        // Configuración del sistema de archivos (Lógica de Negocio)
        private readonly string _uploadPath;
        private readonly int _maxFileSizeMB;
        private readonly int _webpQuality;
        private readonly int _maxImageWidth;
        private readonly int _maxImageHeight;

        public ReparacionFotoService(
            IReparacionFotoRepository repository, // <-- Inyectar Repositorio
            ImageProcessingService imageProcessing,
            ILogger<ReparacionFotoService> logger,
            IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _imageProcessing = imageProcessing ?? throw new ArgumentNullException(nameof(imageProcessing));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // ... (Lógica de configuración del constructor se mantiene igual) ...
            var uploadPathConfig = configuration["FileStorage:UploadPath"];
            _uploadPath = string.IsNullOrEmpty(uploadPathConfig)
                ? Path.Combine(Directory.GetCurrentDirectory(), "CRM", "uploads", "reparaciones")
                : Path.Combine(Directory.GetCurrentDirectory(), uploadPathConfig, "reparaciones");
            _maxFileSizeMB = int.TryParse(configuration["FileStorage:MaxFileSizeMB"], out var maxSize) ? maxSize : 10;
            _webpQuality = int.TryParse(configuration["FileStorage:WebPQuality"], out var quality) ? quality : 80;
            _maxImageWidth = int.TryParse(configuration["FileStorage:MaxImageWidth"], out var width) ? width : 1920;
            _maxImageHeight = int.TryParse(configuration["FileStorage:MaxImageHeight"], out var height) ? height : 1080;
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<ReparacionFotoResponseDto> UploadFotoAsync(
            int reparacionId,
            ReparacionFotoUploadRequestDto dto,
            int usuarioId)
        {
            _logger.LogInformation("Iniciando subida de foto para reparación {ReparacionId}", reparacionId);

            // 1. Validar FK (Delegado al Repositorio)
            var reparacionExists = await _repository.ReparacionExistsAsync(reparacionId);
            if (!reparacionExists)
            {
                throw new KeyNotFoundException($"Reparación con ID {reparacionId} no encontrada.");
            }

            // 2. Validar archivo (Lógica de Negocio)
            // ... (Validaciones de archivo, tamaño, tipo y contenido se mantienen igual) ...
            if (dto.Archivo == null || dto.Archivo.Length == 0) throw new ArgumentException("El archivo es requerido.");
            // ... (más validaciones) ...

            try
            {
                // 3. Generar nombre y ruta (Lógica de Negocio)
                var webpFileName = $"{Guid.NewGuid()}_{SanitizeFileName(dto.Archivo.FileName)}.webp";
                var webpFilePath = Path.Combine(_uploadPath, webpFileName);

                // 4. Procesar y guardar archivo físico (Lógica de Negocio)
                long tamanoBytes;
                int ancho, alto;
                using (var inputStream = dto.Archivo.OpenReadStream())
                {
                    (tamanoBytes, ancho, alto) = await _imageProcessing.ConvertToWebPAsync(
                        inputStream, webpFilePath, _webpQuality, _maxImageWidth, _maxImageHeight);
                }

                // 5. Crear Metadatos (Lógica de Negocio)
                var metadatosJson = JsonSerializer.Serialize(new { /* ... metadatos ... */ });

                // 6. Preparar entidades para el Repositorio
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

                var reparacionFoto = new ReparacionFoto
                {
                    ReparacionId = reparacionId,
                    // DocumentoId se asignará dentro del repositorio
                    Etapa = dto.Etapa,
                    Descripcion = dto.Descripcion,
                    CreadoEn = DateTime.UtcNow
                };

                // 7. Persistir en DB (Delegado al Repositorio)
                // El repositorio maneja la transacción de ambas tablas
                var fotoGuardada = await _repository.CreateFotoInTransactionAsync(reparacionFoto, documento);

                _logger.LogInformation("Foto subida exitosamente ID: {FotoId}", fotoGuardada.Id);

                // 8. Mapear respuesta
                // Pasamos el documento y la foto guardada (que ahora tiene el ID)
                return MapToResponseDto(fotoGuardada, documento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir foto para reparación {ReparacionId}", reparacionId);
                // Aquí se podría añadir lógica para eliminar el archivo físico si la DB falló
                throw new InvalidOperationException("Error al guardar la foto.", ex);
            }
        }

        public async Task<List<ReparacionFotoResponseDto>> GetFotosByReparacionIdAsync(int reparacionId)
        {
            _logger.LogInformation("Consultando fotos de reparación {ReparacionId}", reparacionId);

            // 1. Obtener datos (Delegado al Repositorio)
            // El repositorio ya incluye Documento y CreadoPorUsuario
            var fotos = await _repository.GetByReparacionIdWithDetailsAsync(reparacionId);

            // 2. Mapear (Lógica de Servicio)
            var result = fotos.Select(foto => MapToResponseDto(foto, foto.Documento)).ToList();
            
            _logger.LogInformation("Se encontraron {Count} fotos", result.Count);
            return result;
        }

        public async Task<(Stream fileStream, string contentType, string fileName)?> DownloadFotoAsync(int fotoId)
        {
            _logger.LogInformation("Descargando foto con ID {FotoId}", fotoId);

            // 1. Obtener datos (Delegado al Repositorio)
            var foto = await _repository.GetByIdWithDocumentoAsync(fotoId);

            if (foto?.Documento == null)
            {
                _logger.LogWarning("Foto con ID {FotoId} no encontrada en BD", fotoId);
                return null;
            }

            // 2. Validar archivo físico (Lógica de Servicio)
            if (!File.Exists(foto.Documento.RutaAlmacenamiento))
            {
                _logger.LogError("Archivo físico no encontrado: {Ruta}", foto.Documento.RutaAlmacenamiento);
                return null;
            }

            // 3. Crear Stream (Lógica de Servicio)
            var fileStream = new FileStream(foto.Documento.RutaAlmacenamiento, FileMode.Open, FileAccess.Read, FileShare.Read);

            return (fileStream, foto.Documento.MimeType ?? "image/webp", foto.Documento.NombreArchivo);
        }

        public async Task DeleteFotoAsync(int fotoId)
        {
            _logger.LogInformation("Eliminando foto con ID {FotoId}", fotoId);

            // 1. Obtener entidad para eliminar (Delegado al Repositorio)
            // Usamos el método con tracking del WriteContext
            var foto = await _repository.GetByIdWithDocumentoForDeleteAsync(fotoId);

            if (foto?.Documento == null)
            {
                throw new KeyNotFoundException($"Foto con ID {fotoId} no encontrada.");
            }

            // 2. Eliminar archivo físico (Lógica de Servicio)
            var rutaFisica = foto.Documento.RutaAlmacenamiento;
            if (File.Exists(rutaFisica))
            {
                try
                {
                    File.Delete(rutaFisica);
                    _logger.LogInformation("Archivo físico eliminado: {Ruta}", rutaFisica);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al eliminar archivo físico: {Ruta}. Se continuará con la eliminación de la DB.", rutaFisica);
                }
            }

            // 3. Eliminar de DB (Delegado al Repositorio)
            await _repository.DeleteDocumentAndFotoAsync(foto.Documento);

            _logger.LogInformation("Foto {FotoId} eliminada exitosamente de la DB", fotoId);
        }

        // --- MÉTODOS PRIVADOS ---

        // El mapeo ahora es SÍNCRONO porque el repositorio ya cargó los datos
        private ReparacionFotoResponseDto MapToResponseDto(ReparacionFoto foto, FilesDocumento documento)
        {
            var usuario = documento.CreadoPorUsuario; // Ya viene cargado por el Include del repositorio
            
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

        private string SanitizeFileName(string fileName)
        {
            // ... (Lógica de sanitización se mantiene igual) ...
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrWhiteSpace(sanitized) ? "archivo" : sanitized;
        }
    }
}