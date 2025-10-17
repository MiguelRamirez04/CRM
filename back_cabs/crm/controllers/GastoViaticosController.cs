using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.enums;
using back_cabs.CRM.services.shared;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using back_cabs.CRM.Services.Shared;
using CRM.DTOs.Response;

namespace back_cabs.CRM.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Soporte")]
    public class GastoViaticosController : ControllerBase
    {
        private readonly GastoViaticoService _service;
        private readonly ILogger<GastoViaticosController> _logger;

        public GastoViaticosController(GastoViaticoService service, ILogger<GastoViaticosController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea un nuevo viático con sus detalles.
        /// </summary>
        /// <param name="dto">Datos del viático a crear.</param>
        /// <returns>El viático creado.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(GastoViaticoResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateViatico([FromBody] GastoViaticoCreateRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido en CreateViatico: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.CreateViaticoAsync(dto);
                _logger.LogInformation("Viático creado exitosamente con ID {Id}", result.Id);
                return CreatedAtAction(nameof(GetViaticoById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación en CreateViatico");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en CreateViatico");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un viático específico por ID.
        /// </summary>
        /// <param name="id">ID del viático.</param>
        /// <returns>El viático solicitado.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GastoViaticoResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetViaticoById(int id)
        {
            try
            {
                var result = await _service.GetViaticoByIdAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Viático con ID {Id} no encontrado", id);
                    return NotFound($"Viático con ID {id} no encontrado");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener viático con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Lista viáticos con filtros opcionales y paginación.
        /// </summary>
        /// <param name="usuarioId">Filtrar por ID de usuario.</param>
        /// <param name="tipoViatico">Filtrar por tipo de viático.</param>
        /// <param name="estadoGasto">Filtrar por estado del gasto.</param>
        /// <param name="fechaDesde">Filtrar desde fecha.</param>
        /// <param name="fechaHasta">Filtrar hasta fecha.</param>
        /// <param name="ordenId">Filtrar por ID de orden (solo para tipo ORDEN).</param>
        /// <param name="pageNumber">Número de página (1-based).</param>
        /// <param name="pageSize">Tamaño de página.</param>
        /// <returns>Lista paginada de viáticos.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDto<GastoViaticoResponseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetViaticos(
            [FromQuery] int? usuarioId = null,
            [FromQuery] TipoViatico? tipoViatico = null,
            [FromQuery] EstadoGasto? estadoGasto = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? ordenId = null,
            [FromQuery][Range(1, int.MaxValue)] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetViaticosAsync(
                    usuarioId, tipoViatico, estadoGasto, fechaDesde, fechaHasta, ordenId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar viáticos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza el estado de un viático.
        /// Solo usuarios autorizados pueden cambiar estados.
        /// </summary>
        /// <param name="id">ID del viático.</param>
        /// <param name="dto">Datos de actualización del estado.</param>
        /// <returns>No content si exitoso.</returns>
        [HttpPatch("{id}/estado")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] GastoViaticoUpdateEstadoRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido en UpdateEstado: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue)
                return Unauthorized("Usuario no autenticado");

            try
            {
                await _service.UpdateEstadoAsync(id, dto.EstadoGasto);
                _logger.LogInformation("Estado del viático {Id} actualizado a {Estado} por usuario {UsuarioId}",
                    id, dto.EstadoGasto, usuarioId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Viático no encontrado en UpdateEstado");
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación en UpdateEstado");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en UpdateEstado");
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
}