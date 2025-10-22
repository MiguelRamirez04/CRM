using back_cabs.CRM.DTOs.Files;
using back_cabs.CRM.enums.Files;
using back_cabs.CRM.models.Files;
using back_cabs.CRM.services.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using back_cabs.CRM.services;

namespace back_cabs.CRM.controllers.Files;

/// <summary>
/// Controlador para gestión de archivos del sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FilesController> _logger;
    private readonly ICacheService _cacheService;

    public FilesController(
        IFileStorageService fileStorageService,
        ILogger<FilesController> logger,
        ICacheService cacheService)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Sube un archivo al sistema
    /// </summary>
    /// <param name="request">Datos del archivo a subir</param>
    /// <returns>Información del archivo subido</returns>
    /// <response code="201">Archivo subido exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="401">No autorizado</response>
    /// <response code="413">Archivo demasiado grande</response>
    /// <response code="415">Tipo de archivo no soportado</response>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FileResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(413)]
    [ProducesResponseType(415)]
    public async Task<IActionResult> UploadFile([FromForm] FileUploadRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obtener el ID del usuario del token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                             User.FindFirst("sub") ??
                             User.FindFirst("userId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var usuarioId))
            {
                return Unauthorized("No se pudo obtener el ID del usuario");
            }

            var documento = await _fileStorageService.UploadFileAsync(
                request.Archivo,
                request.EntidadTipo,
                request.EntidadId,
                usuarioId,
                request.Descripcion,
                request.Categoria);

            var response = MapToFileResponseDto(documento);

            return CreatedAtAction(nameof(GetFileById), new { id = documento.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir archivo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Descarga un archivo por su ID
    /// </summary>
    /// <param name="id">ID del documento</param>
    /// <returns>Archivo para descarga</returns>
    /// <response code="200">Archivo descargado exitosamente</response>
    /// <response code="404">Archivo no encontrado</response>
    /// <response code="401">No autorizado</response>
    [HttpGet("{id}/download")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DownloadFile(int id)
    {
        try
        {
            var result = await _fileStorageService.DownloadFileAsync(id);

            if (result == null)
            {
                return NotFound("Archivo no encontrado");
            }

            var (stream, contentType, fileName) = result.Value;

            return File(stream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar archivo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Lista archivos de una entidad específica
    /// </summary>
    /// <param name="entidadTipo">Tipo de entidad</param>
    /// <param name="entidadId">ID de la entidad</param>
    /// <param name="categoria">Filtrar por categoría (opcional)</param>
    /// <param name="soloImagenes">Solo imágenes (opcional)</param>
    /// <returns>Lista de archivos</returns>
    /// <response code="200">Lista obtenida exitosamente</response>
    /// <response code="400">Parámetros inválidos</response>
    /// <response code="401">No autorizado</response>
    [HttpGet("entity")]
    [ProducesResponseType(typeof(IEnumerable<FileResponseDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetFilesByEntity(
        [FromQuery] string entidadTipo,
        [FromQuery] int entidadId,
        [FromQuery] string? categoria = null,
        [FromQuery] bool? soloImagenes = null)
    {
        try
        {
            if (!Enum.TryParse<TipoEntidadDocumento>(entidadTipo, out var tipoEntidad))
            {
                return BadRequest($"Tipo de entidad '{entidadTipo}' no válido");
            }

            var documentos = await _fileStorageService.GetFilesByEntidadAsync(
                tipoEntidad,
                entidadId,
                true); // Solo activos

            // Aplicar filtros adicionales
            if (!string.IsNullOrEmpty(categoria))
            {
                documentos = documentos.Where(d => GetMetadatosValue(d.MetadatosJson, "categoria") == categoria).ToList();
            }

            if (soloImagenes == true)
            {
                documentos = documentos.Where(d => IsImageMimeType(d.MimeType)).ToList();
            }

            var response = documentos.Select(d => MapToFileResponseDto(d));

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener archivos de entidad {Tipo}:{Id}", entidadTipo, entidadId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Valida la integridad de un archivo
    /// </summary>
    /// <param name="id">ID del documento</param>
    /// <returns>Resultado de la validación</returns>
    /// <response code="200">Validación completada</response>
    /// <response code="404">Archivo no encontrado</response>
    /// <response code="401">No autorizado</response>
    [HttpGet("{id}/validate")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ValidateFile(int id)
    {
        try
        {
            var documento = await _fileStorageService.GetFileByIdAsync(id);

            if (documento == null)
            {
                return NotFound("Archivo no encontrado");
            }

            var isValid = await _fileStorageService.ValidateFileIntegrityAsync(id);

            return Ok(new
            {
                documentoId = id,
                nombreArchivo = documento.NombreArchivo,
                esValido = isValid,
                mensaje = isValid ? "Archivo íntegro" : "Archivo corrupto o modificado"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar archivo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene metadatos de un archivo por ID
    /// </summary>
    /// <param name="id">ID del documento</param>
    /// <returns>Metadatos del archivo</returns>
    /// <response code="200">Metadatos obtenidos</response>
    /// <response code="404">Archivo no encontrado</response>
    /// <response code="401">No autorizado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FileResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetFileById(int id)
    {
        try
        {
            string cacheKey = $"file_{id}";

            // Intentar obtener del caché
            var cached = await _cacheService.GetAsync<FileResponseDto>(cacheKey);
            if (cached != null)
            {
                return Ok(cached);
            }

            var documento = await _fileStorageService.GetFileByIdAsync(id);

            if (documento == null)
            {
                return NotFound("Archivo no encontrado");
            }

            var responseDto = MapToFileResponseDto(documento);

            // Guardar en caché
            await _cacheService.SetAsync(cacheKey, responseDto, TimeSpan.FromMinutes(10));

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener archivo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Elimina un archivo (soft delete)
    /// </summary>
    /// <param name="id">ID del documento</param>
    /// <returns>Resultado de la eliminación</returns>
    /// <response code="200">Archivo eliminado exitosamente</response>
    /// <response code="404">Archivo no encontrado</response>
    /// <response code="401">No autorizado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeleteFile(int id)
    {
        try
        {
            // Obtener el ID del usuario del token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                             User.FindFirst("sub") ??
                             User.FindFirst("userId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var usuarioId))
            {
                return Unauthorized("No se pudo obtener el ID del usuario");
            }

            var documento = await _fileStorageService.GetFileByIdAsync(id);

            if (documento == null)
            {
                return NotFound("Archivo no encontrado");
            }

            var result = await _fileStorageService.DeleteFileAsync(id, usuarioId);

            return Ok(new
            {
                documentoId = id,
                eliminado = result,
                mensaje = result ? "Archivo eliminado exitosamente" : "Error al eliminar archivo"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar archivo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #region Métodos Auxiliares

    private FileResponseDto MapToFileResponseDto(FilesDocumento documento)
    {
        return new FileResponseDto
        {
            Id = documento.Id,
            NombreArchivo = documento.NombreArchivo,
            NombreOriginal = documento.NombreOriginal ?? documento.NombreArchivo,
            MimeType = documento.MimeType ?? "application/octet-stream",
            TamanoBytes = documento.TamanoBytes ?? 0,
            TamanoFormateado = FormatFileSize(documento.TamanoBytes ?? 0),
            CreadoEn = documento.CreadoEn,
            CreadoPorNombre = documento.CreadoPorUsuario?.Nombre ?? "Usuario desconocido",
            Descripcion = GetMetadatosValue(documento.MetadatosJson, "descripcion"),
            UrlDescarga = $"{Request.Scheme}://{Request.Host}/api/Files/{documento.Id}/download",
            EsImagen = IsImageMimeType(documento.MimeType),
            ConvertidoAWebP = documento.NombreArchivo.EndsWith(".webp", StringComparison.OrdinalIgnoreCase),
            Categoria = GetMetadatosValue(documento.MetadatosJson, "categoria")
        };
    }

    private string FormatFileSize(long bytes)
    {
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;

        if (bytes >= GB)
            return $"{bytes / (double)GB:F2} GB";
        if (bytes >= MB)
            return $"{bytes / (double)MB:F2} MB";
        if (bytes >= KB)
            return $"{bytes / (double)KB:F2} KB";
        return $"{bytes} bytes";
    }

    private string? GetMetadatosValue(string? metadatosJson, string key)
    {
        if (string.IsNullOrEmpty(metadatosJson))
            return null;

        try
        {
            var metadatos = JsonSerializer.Deserialize<Dictionary<string, object>>(metadatosJson);
            if (metadatos != null && metadatos.TryGetValue(key, out var value))
            {
                return value?.ToString();
            }
        }
        catch
        {
            // Ignorar errores de deserialización
        }

        return null;
    }

    private bool IsImageMimeType(string? mimeType)
    {
        if (string.IsNullOrEmpty(mimeType))
            return false;

        return mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
