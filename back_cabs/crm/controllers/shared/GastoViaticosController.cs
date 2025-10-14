using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Services.Shared;
using back_cabs.CRM.enums;
using System.ComponentModel.DataAnnotations;

namespace CRM.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Recepcion")]
    public class GastoViaticosController : ControllerBase
    {
        private readonly GastoViaticoService _viaticoService;
        private readonly ILogger<GastoViaticosController> _logger;

        public GastoViaticosController(GastoViaticoService viaticoService, ILogger<GastoViaticosController> logger)
        {
            _viaticoService = viaticoService;
            _logger = logger;
        }

        /// <summary>
        /// Crea un nuevo viático (POST /api/viaticos).
        /// </summary>
        /// <param name="request">Datos del viático a crear.</param>
        /// <returns>El viático creado con ID asignado.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(GastoViaticoResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateViatico([FromBody] GastoViaticoCreateRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creando nuevo viático para usuario {UsuarioId}", request.UsuarioId);

                var result = await _viaticoService.CreateViaticoAsync(request);

                _logger.LogInformation("Viático creado exitosamente con ID {ViaticoId}", result.Id);
                return CreatedAtAction(nameof(GetViaticoById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear viático");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Lista viáticos con filtros opcionales (GET /api/viaticos).
        /// </summary>
        /// <param name="tipo">Filtrar por tipo (opcional).</param>
        /// <param name="usuarioId">Filtrar por usuario (opcional).</param>
        /// <param name="estado">Filtrar por estado (opcional).</param>
        /// <param name="fechaDesde">Filtrar desde fecha (opcional).</param>
        /// <param name="fechaHasta">Filtrar hasta fecha (opcional).</param>
        /// <returns>Lista de viáticos.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GastoViaticoResponseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetViaticos(
            [FromQuery] TipoViatico? tipo = null,
            [FromQuery] int? usuarioId = null,
            [FromQuery] EstadoGasto? estado = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                _logger.LogInformation($"Consultando viáticos con filtros: Tipo={tipo}, UsuarioId={usuarioId}, Estado={estado}, FechaDesde={fechaDesde}, FechaHasta={fechaHasta}");

                var result = await _viaticoService.GetViaticosAsync(tipo, usuarioId, estado, fechaDesde, fechaHasta);

                _logger.LogInformation("Se encontraron {Count} viáticos", result.Count());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar viáticos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un viático específico por ID (GET /api/viaticos/{id}).
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
                _logger.LogInformation("Consultando viático con ID {ViaticoId}", id);

                var result = await _viaticoService.GetViaticoByIdAsync(id);

                if (result == null)
                {
                    _logger.LogWarning("Viático con ID {ViaticoId} no encontrado", id);
                    return NotFound($"Viático con ID {id} no encontrado");
                }

                _logger.LogInformation("Viático con ID {ViaticoId} encontrado", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar viático con ID {ViaticoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza el estado de un viático (PATCH /api/viaticos/{id}/estado).
        /// </summary>
        /// <param name="id">ID del viático.</param>
        /// <param name="request">Nuevo estado.</param>
        /// <returns>No content si exitoso.</returns>
        [HttpPatch("{id}/estado")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] UpdateEstadoViaticoRequestDto request)
        {
            try
            {
                _logger.LogInformation("Actualizando estado de viático {ViaticoId} a {Estado}", id, request.Estado);

                await _viaticoService.UpdateEstadoAsync(id, request.Estado);

                _logger.LogInformation("Estado de viático {ViaticoId} actualizado exitosamente", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Viático con ID {ViaticoId} no encontrado", id);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Estado inválido para viático {ViaticoId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de viático {ViaticoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }

    /// <summary>
    /// DTO para actualizar el estado de un viático.
    /// </summary>
    public class UpdateEstadoViaticoRequestDto
    {
        [Required]
        public EstadoGasto Estado { get; set; }
    }
}