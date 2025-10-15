using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.services.Soporte;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.controllers
{
    /// <summary>
    /// Controller para gestionar fotos de reparaciones.
    /// Endpoints: POST (subir), GET (listar/descargar), DELETE (eliminar).
    /// </summary>
    [ApiController]
    [Route("api/reparaciones/{reparacionId}/fotos")]
    [Authorize(Roles = "SOPORTE,ADMINISTRACION")]
    public class ReparacionFotosController : ControllerBase
    {
        private readonly ReparacionFotoService _service;
        private readonly ILogger<ReparacionFotosController> _logger;

        public ReparacionFotosController(
            ReparacionFotoService service, 
            ILogger<ReparacionFotosController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Subir una foto a una reparación (convierte automáticamente a WebP).
        /// </summary>
        /// <param name="reparacionId">ID de la reparación.</param>
        /// <param name="dto">DTO con el archivo y metadatos.</param>
        /// <returns>Información de la foto creada.</returns>
        /// <remarks>
        /// Formato aceptado: multipart/form-data
        /// 
        /// Tipos de imagen permitidos: JPEG, PNG, WebP, GIF
        /// 
        /// Tamaño máximo: 10MB (configurable)
        /// 
        /// La imagen se convertirá automáticamente a formato WebP para optimizar tamaño y rendimiento.
        /// </remarks>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ReparacionFotoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadFoto(
            int reparacionId,
            [FromForm] ReparacionFotoUploadRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido al subir foto para reparación {ReparacionId}", reparacionId);
                return BadRequest(ModelState);
            }

            try
            {
                var usuarioId = GetCurrentUserId();
                _logger.LogInformation("Usuario {UsuarioId} subiendo foto a reparación {ReparacionId}", 
                    usuarioId, reparacionId);

                var result = await _service.UploadFotoAsync(reparacionId, dto, usuarioId);
                
                return CreatedAtAction(
                    nameof(DownloadFoto), 
                    new { reparacionId, id = result.Id }, 
                    result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Reparación no encontrada: {ReparacionId}", reparacionId);
                return NotFound(new { Error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al subir foto");
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al subir foto");
                return StatusCode(500, new { Error = "Error interno al subir foto. Por favor, inténtelo de nuevo." });
            }
        }

        /// <summary>
        /// Obtener todas las fotos de una reparación.
        /// </summary>
        /// <param name="reparacionId">ID de la reparación.</param>
        /// <returns>Lista de fotos con sus metadatos.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ReparacionFotoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFotos(int reparacionId)
        {
            try
            {
                _logger.LogInformation("Obteniendo fotos de reparación {ReparacionId}", reparacionId);
                var fotos = await _service.GetFotosByReparacionIdAsync(reparacionId);
                return Ok(fotos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fotos de reparación {ReparacionId}", reparacionId);
                return StatusCode(500, new { Error = "Error al obtener fotos." });
            }
        }

        /// <summary>
        /// Descargar una foto específica.
        /// </summary>
        /// <param name="reparacionId">ID de la reparación.</param>
        /// <param name="id">ID de la foto.</param>
        /// <returns>Archivo de imagen en formato WebP.</returns>
        [HttpGet("{id}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadFoto(int reparacionId, int id)
        {
            try
            {
                _logger.LogInformation("Descargando foto {FotoId} de reparación {ReparacionId}", id, reparacionId);
                
                var result = await _service.DownloadFotoAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Foto {FotoId} no encontrada", id);
                    return NotFound(new { Error = "Foto no encontrada." });
                }

                return File(result.Value.fileStream, result.Value.contentType, result.Value.fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar foto {FotoId}", id);
                return StatusCode(500, new { Error = "Error al descargar foto." });
            }
        }

        /// <summary>
        /// Eliminar una foto.
        /// </summary>
        /// <param name="reparacionId">ID de la reparación.</param>
        /// <param name="id">ID de la foto a eliminar.</param>
        /// <returns>NoContent si exitoso.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFoto(int reparacionId, int id)
        {
            try
            {
                var usuarioId = GetCurrentUserId();
                _logger.LogInformation("Usuario {UsuarioId} eliminando foto {FotoId} de reparación {ReparacionId}", 
                    usuarioId, id, reparacionId);

                await _service.DeleteFotoAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Foto {FotoId} no encontrada", id);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar foto {FotoId}", id);
                return StatusCode(500, new { Error = "Error al eliminar foto." });
            }
        }

        /// <summary>
        /// Obtiene el ID del usuario actual desde el token JWT.
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                _logger.LogError("No se pudo obtener el ID del usuario del token JWT");
                throw new UnauthorizedAccessException("Usuario no autenticado.");
            }
            return userId;
        }
    }
}
