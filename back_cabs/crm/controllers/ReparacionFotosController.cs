using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.services;
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
        private readonly ICacheService _cache;

        public ReparacionFotosController(
            ReparacionFotoService service, 
            ILogger<ReparacionFotosController> logger,
            ICacheService cache)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache;
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

                var created = await _service.UploadFotoAsync(reparacionId, dto, usuarioId);
                // invalidar lista de fotos de la reparación
                await _cache.RemoveAsync($"rep:{reparacionId}:fotos:list");

                return CreatedAtAction(
                    nameof(DownloadFoto), 
                    new { reparacionId, id = created.Id }, 
                    created);
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
            var key = $"rep:{reparacionId}:fotos:list";
            var cached = await _cache.GetAsync<List<ReparacionFotoResponseDto>>(key);
            if (cached != null) return Ok(cached);

            var fotos = await _service.GetFotosByReparacionAsync(reparacionId);
            // cachear solo metadatos/DTOs, no bytes
            await _cache.SetAsync(key, fotos, TimeSpan.FromMinutes(10));
            return Ok(fotos);
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
            // No cachear bytes por seguridad; si quisieras mejorar performance, cache metadata o usar CDN/local FS
            var result = await _service.GetFotoFileAsync(id);
            if (result == null) return NotFound();
            return File(result.Value.FileBytes, result.Value.MimeType, result.Value.FileName);
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

                var deleted = await _service.DeleteFotoAsync(id, usuarioId);
                if (!deleted)
                {
                    return NotFound(new { Error = "Foto no encontrada." });
                }
                return NoContent();
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
