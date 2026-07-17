using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.enums;
using back_cabs.CRM.services.shared;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace back_cabs.CRM.controllers.shared
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Solo requiere autenticación, sin restricción de roles
    public class EjecucionOrdenController : ControllerBase
    {
        private readonly EjecucionOrdenService _service;
        private readonly ILogger<EjecucionOrdenController> _logger;

        public EjecucionOrdenController(EjecucionOrdenService service, ILogger<EjecucionOrdenController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea una nueva ejecución de orden de trabajo.
        /// </summary>
        /// <param name="dto">Datos de la ejecución a crear.</param>
        /// <returns>La ejecución creada.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EjecucionOrdenResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateEjecucion([FromBody] EjecucionOrdenCreateRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido en CreateEjecucion: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.CreateEjecucionAsync(dto);
                _logger.LogInformation("Ejecución creada exitosamente con ID {Id}", result.Id);
                return CreatedAtAction(nameof(GetEjecucionById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación en CreateEjecucion");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en CreateEjecucion");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene una ejecución específica por ID.
        /// </summary>
        /// <param name="id">ID de la ejecución.</param>
        /// <returns>La ejecución solicitada.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EjecucionOrdenResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetEjecucionById(int id)
        {
            try
            {
                var result = await _service.GetEjecucionByIdAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Ejecución con ID {Id} no encontrada", id);
                    return NotFound($"Ejecución con ID {id} no encontrada");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ejecución con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Lista ejecuciones con filtros opcionales.
        /// </summary>
        /// <param name="ordenId">Filtrar por ID de orden.</param>
        /// <param name="tecnicoId">Filtrar por ID de técnico.</param>
        /// <param name="tipoEjecucion">Filtrar por tipo de ejecución.</param>
        /// <param name="fechaDesde">Filtrar desde fecha de inicio.</param>
        /// <param name="fechaHasta">Filtrar hasta fecha de inicio.</param>
        /// <returns>Lista de ejecuciones.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<EjecucionOrdenResponseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetEjecuciones(
            [FromQuery] int? ordenId = null,
            [FromQuery] int? tecnicoId = null,
            [FromQuery] TipoEjecucion? tipoEjecucion = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var result = await _service.GetEjecucionesAsync(ordenId, tecnicoId, tipoEjecucion, fechaDesde, fechaHasta);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar ejecuciones");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza una ejecución (ej. finalizar con HrFin, KmFinal).
        /// Solo el técnico asignado puede actualizar.
        /// </summary>
        /// <param name="id">ID de la ejecución.</param>
        /// <param name="dto">Campos a actualizar.</param>
        /// <returns>No content si exitoso.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateEjecucion(int id, [FromBody] EjecucionOrdenUpdateRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido en UpdateEjecucion: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue)
                return Unauthorized("Usuario no autenticado");

            try
            {
                await _service.UpdateEjecucionAsync(id, usuarioId.Value, dto);
                _logger.LogInformation("Ejecución {Id} actualizada por usuario {UsuarioId}", id, usuarioId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ejecución no encontrada en UpdateEjecucion");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en UpdateEjecucion");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Delega una ejecución a otro técnico.
        /// </summary>
        /// <param name="id">ID de la ejecución.</param>
        /// <param name="dto">Datos de la delegación.</param>
        /// <returns>No content si exitoso.</returns>
        [HttpPatch("{id}/delegate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DelegateEjecucion(int id, [FromBody] DelegateEjecucionRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido en DelegateEjecucion: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue)
                return Unauthorized("Usuario no autenticado");

            try
            {
                await _service.DelegateEjecucionAsync(id, dto.NuevoTecnicoId, usuarioId.Value, dto.Motivo);
                _logger.LogInformation("Ejecución {Id} delegada por usuario {UsuarioId}", id, usuarioId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación en DelegateEjecucion");
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ejecución no encontrada en DelegateEjecucion");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en DelegateEjecucion");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // =====================================================================================
        // ENDPOINTS PARA EL NUEVO FLUJO DE EJECUCIONES
        // =====================================================================================

        /// <summary>
        /// Obtiene las tareas pendientes disponibles para técnicos SOPORTE.
        /// Solo accesible para usuarios con rol SOPORTE.
        /// </summary>
        /// <returns>Lista de tareas pendientes.</returns>
        [HttpGet("tareas-pendientes")]
        [Authorize(Roles = "SOPORTE,ADMINISTRADOR")] // Solo técnicos pueden ver tareas pendientes
        [ProducesResponseType(typeof(List<TareaPendienteDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTareasPendientes()
        {
            try
            {
                var tareas = await _service.ObtenerTareasPendientesAsync();
                _logger.LogInformation("Se obtuvieron {Count} tareas pendientes", tareas.Count);
                return Ok(tareas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tareas pendientes");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Permite a un técnico SOPORTE tomar una tarea pendiente.
        /// Crea automáticamente una ejecución para la orden.
        /// </summary>
        /// <param name="request">Datos para tomar la tarea.</param>
        /// <returns>Ejecución creada para la tarea tomada.</returns>
        [HttpPost("tomar-tarea")]
        [Authorize(Roles = "SOPORTE,ADMINISTRADOR")] // Solo técnicos pueden tomar tareas
        [ProducesResponseType(typeof(EjecucionOrdenResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> TomarTarea([FromBody] back_cabs.CRM.services.shared.TomarTareaRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido en TomarTarea: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            try
            {
                // Obtener el ID del usuario actual desde el token JWT
                var userIdClaim = User.FindFirst("id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var usuarioId))
                {
                    _logger.LogWarning("No se pudo obtener el ID del usuario del token JWT");
                    return Unauthorized(new { message = "Usuario no autenticado correctamente" });
                }

                var ejecucion = await _service.TomarTareaAsync(request, usuarioId);
                _logger.LogInformation("Usuario {UsuarioId} tomó la tarea de orden {OrdenId}, ejecución {EjecucionId} creada",
                    usuarioId, request.OrdenId, ejecucion.Id);

                return CreatedAtAction(nameof(GetEjecucionById), new { id = ejecucion.Id }, ejecucion);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado para tomar tarea");
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Datos inválidos en TomarTarea: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida en TomarTarea: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al tomar tarea");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene el ID del usuario actual desde el token JWT.
        /// </summary>
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    /// <summary>
    /// DTO para solicitar delegación de ejecución.
    /// </summary>
    public class DelegateEjecucionRequestDto
    {
        [Required(ErrorMessage = "El ID del nuevo técnico es obligatorio.")]
        public int NuevoTecnicoId { get; set; }

        [Required(ErrorMessage = "El motivo de la delegación es obligatorio.")]
        [MaxLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para solicitar tomar una tarea pendiente.
    /// </summary>
    public class TomarTareaRequestDto
    {
        [Required(ErrorMessage = "El ID de la orden es obligatorio.")]
        public int OrdenId { get; set; }

        [Required(ErrorMessage = "El ID de la ejecución es obligatorio.")]
        public int EjecucionId { get; set; }
    }

    /// <summary>
    /// DTO para representar una tarea pendiente.
    /// </summary>
    public class TareaPendienteDto
    {
        public int OrdenId { get; set; }
        public int EjecucionId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string TecnicoAsignado { get; set; } = string.Empty;
    }
}
