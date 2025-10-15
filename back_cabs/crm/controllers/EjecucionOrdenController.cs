using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.enums;
using back_cabs.CRM.services.shared;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace back_cabs.CRM.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Soporte")]
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado en UpdateEjecucion");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en UpdateEjecucion");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Delega una ejecución a otro técnico.
        /// Solo usuarios con rol SOPORTE pueden delegar.
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado en DelegateEjecucion");
                return Unauthorized(ex.Message);
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
}