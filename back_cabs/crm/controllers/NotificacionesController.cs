// =====================================================================================
// CONTROLADOR NOTIFICACIONES - NotificacionesController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE CONTROLADOR?
// Gestiona las operaciones CRUD de notificaciones del sistema.
// Proporciona endpoints REST para consultar y gestionar notificaciones.
//
// ENDPOINTS:
// - GET /api/Notificaciones - Obtener notificaciones del usuario actual
// - PUT /api/Notificaciones/{id}/leida - Marcar notificación como leída
// - DELETE /api/Notificaciones/{id} - Eliminar notificación
//
// =====================================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.services;
using back_cabs.CRM.models;

namespace back_cabs.CRM.controllers
{
    /// <summary>
    /// Controlador para gestión de notificaciones del sistema
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacionesController : ControllerBase
    {
        private readonly INotificacionService _notificacionService;
        private readonly ILogger<NotificacionesController> _logger;

        public NotificacionesController(
            INotificacionService notificacionService,
            ILogger<NotificacionesController> logger)
        {
            _notificacionService = notificacionService ?? throw new ArgumentNullException(nameof(notificacionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtener todas las notificaciones del usuario actual
        /// </summary>
        /// <returns>Lista de notificaciones del usuario</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<NotificacionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotificaciones()
        {
            try
            {
                // Obtener ID del usuario del JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Usuario no válido" });
                }

                var notificaciones = await _notificacionService.ObtenerNotificacionesAsync(userId);

                _logger.LogInformation(
                    "Se obtuvieron {Count} notificaciones para el usuario {UserId}",
                    notificaciones.Count(),
                    userId);

                return Ok(notificaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones del usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Marcar una notificación como leída
        /// </summary>
        /// <param name="id">ID de la notificación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPut("{id}/leida")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarcarComoLeida(int id)
        {
            try
            {
                // Obtener ID del usuario del JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Usuario no válido" });
                }

                var result = await _notificacionService.MarcarComoLeidaAsync(id, userId);

                if (!result)
                {
                    return NotFound(new { message = "Notificación no encontrada" });
                }

                _logger.LogInformation("Notificación {Id} marcada como leída", id);
                return Ok(new { message = "Notificación marcada como leída" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar notificación como leída: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Eliminar una notificación
        /// </summary>
        /// <param name="id">ID de la notificación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarNotificacion(int id)
        {
            try
            {
                var result = await _notificacionService.EliminarNotificacionAsync(id);

                if (!result)
                {
                    return NotFound(new { message = "Notificación no encontrada" });
                }

                _logger.LogInformation("Notificación {Id} eliminada", id);
                return Ok(new { message = "Notificación eliminada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar notificación: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener conteo de notificaciones no leídas
        /// </summary>
        /// <returns>Conteo de notificaciones no leídas</returns>
        [HttpGet("no-leidas/count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotificacionesNoLeidasCount()
        {
            try
            {
                // Obtener ID del usuario del JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Usuario no válido" });
                }

                var count = await _notificacionService.ObtenerConteoNoLeidasAsync(userId);

                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteo de notificaciones no leídas");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}