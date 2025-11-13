using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.services; // Para ICacheService
using back_cabs.CRM.services.Soporte; // Para ReparacionFotoService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO; // Necesario para KeyNotFoundException

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

        // ... (POST UploadFoto se mantiene igual, ya era correcto) ...
        
        /// <summary>
        /// Subir una foto a una reparación (convierte automáticamente a WebP).
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ReparacionFotoResponseDto), StatusCodes.Status201Created)]
        // ... (otros ProduceResponseType)
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
                    nameof(DownloadFoto), // Apunta al método de descarga corregido
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
        [HttpGet]
        [ProducesResponseType(typeof(List<ReparacionFotoResponseDto>), StatusCodes.Status200OK)]
        // ... (otros ProduceResponseType)
        public async Task<IActionResult> GetFotos(int reparacionId)
        {
            var key = $"rep:{reparacionId}:fotos:list";
            var cached = await _cache.GetAsync<List<ReparacionFotoResponseDto>>(key);
            if (cached != null) return Ok(cached);

            // ✅ CORRECCIÓN 1: Llamar al método correcto (GetFotosByReparacionIdAsync)
            var fotos = await _service.GetFotosByReparacionIdAsync(reparacionId);
            
            await _cache.SetAsync(key, fotos, TimeSpan.FromMinutes(10));
            return Ok(fotos);
        }

        /// <summary>
        /// Descargar una foto específica.
        /// </summary>
        [HttpGet("{id}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        // ... (otros ProduceResponseType)
        public async Task<IActionResult> DownloadFoto(int reparacionId, int id)
        {
            // ✅ CORRECCIÓN 2: Llamar al método correcto (DownloadFotoAsync)
            var result = await _service.DownloadFotoAsync(id); 
            
            if (result == null) return NotFound();

            // ✅ CORRECCIÓN 2.1: Usar los nombres de tupla correctos (fileStream, contentType, fileName)
            return File(result.Value.fileStream, result.Value.contentType, result.Value.fileName);
        }

        /// <summary>
        /// Eliminar una foto.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        // ... (otros ProduceResponseType)
        public async Task<IActionResult> DeleteFoto(int reparacionId, int id)
        {
            try
            {
                var usuarioId = GetCurrentUserId();
                _logger.LogInformation("Usuario {UsuarioId} eliminando foto {FotoId} de reparación {ReparacionId}",
                    usuarioId, id, reparacionId);

                // ✅ CORRECCIÓN 3: Llamar a DeleteFotoAsync solo con el 'id' de la foto
                await _service.DeleteFotoAsync(id);
                
                // Invalidar caché tras eliminación exitosa
                await _cache.RemoveAsync($"rep:{reparacionId}:fotos:list");

                return NoContent();
            }
            catch (KeyNotFoundException ex) // Capturar la excepción si el servicio no encuentra la foto
            {
                _logger.LogWarning(ex, "Foto no encontrada para eliminar: {FotoId}", id);
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