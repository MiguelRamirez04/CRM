using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_cabs.CRM.contexts;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.DTOs.shared;
using back_cabs.CRM.services.Files;
using back_cabs.CRM.enums.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using back_cabs.CRM.Interfaces;

namespace back_cabs.CRM.services.shared
{
    /// <summary>
    /// Servicio para gestionar fotos de evaluaciones.
    /// Delega el almacenamiento físico a FileStorageService.
    /// </summary>
    public class FotosEvaluacionService : IFotosEvaluacion
    {
        private readonly ReadOnlyContext _readContext;
        private readonly WriteContext _writeContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<FotosEvaluacionService> _logger;

        public FotosEvaluacionService(
            ReadOnlyContext readContext,
            WriteContext writeContext,
            IFileStorageService fileStorageService,
            ILogger<FotosEvaluacionService> logger)
        {
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Create

        /// <summary>
        /// Sube una foto para una evaluación
        /// </summary>
        public async Task<EvaluacionFotoResponseDto> CreateFotoAsync(
            EvaluacionFotoRequestDto dto, 
            int usuarioId)
        {
            _logger.LogInformation(
                "Subiendo foto para detalle evaluacion {DetalleId} por usuario {UsuarioId}", 
                dto.DetalleId, usuarioId);

            // Validar que el detalle de evaluación existe
            var detalleExists = await _readContext.EvaluacionesDetalles
                .AnyAsync(d => d.Id == dto.DetalleId);
            
            if (!detalleExists)
            {
                throw new KeyNotFoundException($"Detalle de evaluacion con ID {dto.DetalleId} no encontrado");
            }

            // Delegar subida a FileStorageService
            var documento = await _fileStorageService.UploadFileAsync(
                dto.Archivo,
                TipoEntidadDocumento.Evaluacion,
                dto.DetalleId,
                usuarioId,
                dto.Descripcion,
                dto.Tipo
            );

            // Verificar si ya existe la relación DetalleId + DocumentoId
            var fotoExistente = await _readContext.EvaluacionesFotos
                .FirstOrDefaultAsync(f => 
                    f.DetalleId == dto.DetalleId && 
                    f.DocumentoId == documento.Id);

            if (fotoExistente != null)
            {
                _logger.LogInformation(
                    "Relación foto existente encontrada - Retornando registro {FotoId} (DetalleId: {DetalleId}, DocumentoId: {DocumentoId})",
                    fotoExistente.Id, dto.DetalleId, documento.Id);

                return new EvaluacionFotoResponseDto
                {
                    Id = fotoExistente.Id,
                    DetalleId = fotoExistente.DetalleId,
                    DocumentoId = documento.Id,
                    NombreArchivo = documento.NombreArchivo,
                    MimeType = documento.MimeType,
                    TamanoBytes = documento.TamanoBytes,
                    Tipo = fotoExistente.Tipo,
                    Descripcion = fotoExistente.Descripcion,
                    CreadoEn = fotoExistente.CreadoEn
                };
            }

            // Crear registro en evaluacion_fotos
            var fotoEvaluacion = new EvaluacionFoto
            {
                DetalleId = dto.DetalleId,
                DocumentoId = documento.Id,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                CreadoEn = DateTime.UtcNow
            };

            await _writeContext.EvaluacionesFotos.AddAsync(fotoEvaluacion);
            await _writeContext.SaveChangesAsync();

            _logger.LogInformation(
                "Foto {FotoId} creada con documento {DocumentoId}", 
                fotoEvaluacion.Id, documento.Id);

            return new EvaluacionFotoResponseDto
            {
                Id = fotoEvaluacion.Id,
                DetalleId = fotoEvaluacion.DetalleId,
                DocumentoId = documento.Id,
                NombreArchivo = documento.NombreArchivo,
                MimeType = documento.MimeType,
                TamanoBytes = documento.TamanoBytes,
                Tipo = fotoEvaluacion.Tipo,
                Descripcion = fotoEvaluacion.Descripcion,
                CreadoEn = fotoEvaluacion.CreadoEn,
                UrlDescarga = $"/api/FotosEvaluacion/{fotoEvaluacion.Id}/download"
            };
        }

        #endregion

        #region Read

        /// <summary>
        /// Obtiene todas las fotos de evaluación
        /// </summary>
        public async Task<List<EvaluacionFotoResponseDto>> GetAllFotosAsync()
        {
            var fotos = await _readContext.EvaluacionesFotos
                .Include(f => f.Documento)
                .Where(f => f.Documento != null && f.Documento.Activo)
                .OrderByDescending(f => f.CreadoEn)
                .ToListAsync();

            return fotos.Select(f => new EvaluacionFotoResponseDto
            {
                Id = f.Id,
                DetalleId = f.DetalleId,
                DocumentoId = f.DocumentoId,
                NombreArchivo = f.Documento?.NombreArchivo ?? "desconocido",
                MimeType = f.Documento?.MimeType ?? "application/octet-stream",
                TamanoBytes = f.Documento?.TamanoBytes ?? 0,
                Tipo = f.Tipo,
                Descripcion = f.Descripcion,
                CreadoEn = f.CreadoEn,
                UrlDescarga = $"/api/FotosEvaluacion/{f.Id}/download"
            }).ToList();
        }

        /// <summary>
        /// Obtiene fotos por detalle de evaluación
        /// </summary>
        public async Task<List<EvaluacionFotoResponseDto>> GetFotosByDetalleAsync(int detalleId)
        {
            var fotos = await _readContext.EvaluacionesFotos
                .Include(f => f.Documento)
                .Where(f => f.DetalleId == detalleId 
                    && f.Documento != null 
                    && f.Documento.Activo)
                .OrderByDescending(f => f.CreadoEn)
                .ToListAsync();

            return fotos.Select(f => new EvaluacionFotoResponseDto
            {
                Id = f.Id,
                DetalleId = f.DetalleId,
                DocumentoId = f.DocumentoId,
                NombreArchivo = f.Documento?.NombreArchivo ?? "desconocido",
                MimeType = f.Documento?.MimeType ?? "application/octet-stream",
                TamanoBytes = f.Documento?.TamanoBytes ?? 0,
                Tipo = f.Tipo,
                Descripcion = f.Descripcion,
                CreadoEn = f.CreadoEn,
                UrlDescarga = $"/api/FotosEvaluacion/{f.Id}/download"
            }).ToList();
        }

        /// <summary>
        /// Obtiene una foto por su ID
        /// </summary>
        public async Task<EvaluacionFotoResponseDto?> GetFotoByIdAsync(int fotoId)
        {
            var foto = await _readContext.EvaluacionesFotos
                .Include(f => f.Documento)
                .FirstOrDefaultAsync(f => f.Id == fotoId);

            if (foto == null) return null;

            return new EvaluacionFotoResponseDto
            {
                Id = foto.Id,
                DetalleId = foto.DetalleId,
                DocumentoId = foto.DocumentoId,
                NombreArchivo = foto.Documento?.NombreArchivo ?? "desconocido",
                MimeType = foto.Documento?.MimeType ?? "application/octet-stream",
                TamanoBytes = foto.Documento?.TamanoBytes ?? 0,
                Tipo = foto.Tipo,
                Descripcion = foto.Descripcion,
                CreadoEn = foto.CreadoEn,
                UrlDescarga = $"/api/FotosEvaluacion/{foto.Id}/download"
            };
        }

        /// <summary>
        /// Descarga el archivo de una foto
        /// </summary>
        public async Task<(byte[] FileBytes, string FileName, string MimeType)?> GetFotoFileAsync(int fotoId)
        {
            var foto = await _readContext.EvaluacionesFotos
                .Include(f => f.Documento)
                .FirstOrDefaultAsync(f => f.Id == fotoId);

            if (foto?.Documento == null)
            {
                _logger.LogWarning("Documento no encontrado para foto {FotoId}", fotoId);
                return null;
            }

            // Delegar descarga a FileStorageService
            var result = await _fileStorageService.DownloadFileAsync(foto.DocumentoId);

            if (result == null)
            {
                _logger.LogError("Error al descargar documento {DocumentoId}", foto.DocumentoId);
                return null;
            }

            using (var memoryStream = new System.IO.MemoryStream())
            {
                await result.Value.stream.CopyToAsync(memoryStream);
                return (memoryStream.ToArray(), result.Value.fileName, result.Value.contentType);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// Elimina una foto (soft delete del documento)
        /// </summary>
        public async Task<bool> DeleteFotoAsync(int fotoId, int usuarioId)
        {
            _logger.LogInformation("Eliminando foto {FotoId} por usuario {UsuarioId}", fotoId, usuarioId);

            var fotoAEliminar = await _writeContext.EvaluacionesFotos
                .FirstOrDefaultAsync(f => f.Id == fotoId);

            if (fotoAEliminar == null)
            {
                _logger.LogWarning("Foto {FotoId} no encontrada", fotoId);
                return false;
            }

            // Delegar eliminación a FileStorageService (soft delete)
            var deleted = await _fileStorageService.DeleteFileAsync(fotoAEliminar.DocumentoId, usuarioId);

            if (!deleted)
            {
                _logger.LogError("Error al eliminar documento {DocumentoId}", fotoAEliminar.DocumentoId);
                return false;
            }

            // Eliminar registro de evaluacion_fotos
            _writeContext.EvaluacionesFotos.Remove(fotoAEliminar);
            await _writeContext.SaveChangesAsync();

            _logger.LogInformation("Foto {FotoId} eliminada exitosamente", fotoId);
            return true;
        }

        #endregion
    }
}
