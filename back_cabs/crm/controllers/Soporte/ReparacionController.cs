using back_cabs.CRM.DTOs.Soporte;
using back_cabs.CRM.services.Soporte;
using back_cabs.CRM.Middleware; // Asumimos que aquí está UtilidadesManejoErrores
using back_cabs.CRM.enums; // Asumimos que aquí está TipoError
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using back_cabs.CRM.models.Soporte;

namespace back_cabs.CRM.DTOs.Soporte
{
    ///<summary>
    /// Controlador para gestionar las reparaciones.
    /// Proporciona endpoints para crear, actualizar, obtener y eliminar reparaciones.
    /// </summary>
    ///<remarks>
    /// Rutas base: /api/soporte/reparaciones
    /// </remarks>
    [ApiController]
    [Route("api/soporte/reparaciones")]
    [Produces("application/json")]
    [Authorize]
    public class ReparacionController : ControllerBase
    {
        private readonly ReparacionService _reparacionService;
        private readonly ILogger<ReparacionController> _logger;

        public ReparacionController(ReparacionService reparacionService,
        ILogger<ReparacionController> logger)
        {
            _reparacionService = reparacionService ?? throw new ArgumentNullException(nameof(reparacionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /* --- ENDPOINTS (FUNCIONALIDADES) ---*/
        /// <summary>
        /// Obtiene la lista completa de Reparaciones.
        /// </summary>
        /// <param name="skip">Número de registros a saltar (paginación).</param>
        /// <param name="take">Número de registros a obtener (paginación).</param>
        /// <returns>Lista de ReparacionResponseDto.</returns>
        /// <response code="200">Lista obtenida exitosamente.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReparacionResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]

        public async Task<ActionResult<List<ReparacionResponseDto>>> GetReparaciones()
        {
            try
            {
                var reparaciones = await _reparacionService.ObtenerReparacionesAsync();
                return Ok(reparaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de reparaciones}", ex);
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error de lectura",
                    "No se pudieron obtener las reparaciones."));
            }
        }

        /// <summary>
        /// Obtiene una Reparación por su ID.
        /// </summary>
        /// <param name="id">El ID único de la reparación.</param>
        /// <returns>ReparacionResponseDto.</returns>
        /// <response code="200">Reparación encontrada exitosamente.</response>
        /// <response code="404">Reparación no encontrada.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReparacionResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<ReparacionResponseDto>> GetReparacionPorId(int id)
        {
            try
            {
                _logger.LogInformation("Buscando reparación con ID: {Id}", id);

                var reparacion = await _reparacionService.ObtenerReparacionPorIdAsync(id);

                if (reparacion == null)
                {
                    _logger.LogWarning("Reparación con ID {Id} no encontrada", id);
                    return NotFound(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorNoEncontrado,
                        "No encontrado",
                        $"No existe una reparación con ID {id}"));
                }

                return Ok(reparacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reparación con ID: {Id}", id);
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error de lectura",
                    "No se pudo obtener la reparación."));
            }
        }

        /// <summary>
        /// Crea una nueva Reparación asociada a una Orden de Trabajo.
        /// </summary>
        /// <param name="requestDto">DTO con los datos de creación.</param>
        /// <returns>La Reparacion recién creada.</returns>
        /// <response code="201">Reparación creada exitosamente.</response>
        /// <response code="400">Datos de entrada inválidos (Validación o FKs).</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ReparacionResponseDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<ReparacionResponseDto>> CrearReparacion([FromBody] ReparacionCreacionRequestDto requestDto)
        {
            try
            {
                _logger.LogInformation("Creando nueva reparación para la orden ID: {OrdenId}", requestDto.OrdenId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ReparacionCreada = await _reparacionService.CrearReparacionAsync(requestDto);

                _logger.LogInformation("Reparacion creada con ID: {Id}", ReparacionCreada.Id);

                return CreatedAtAction(nameof(GetReparacionPorId), new { id = ReparacionCreada.Id }, ReparacionCreada);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Validación de Clave Foránea fallida al crear reparación.");
                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Referencia no encontrada",
                    ex.Message));
            }
            catch (ArgumentException ex)
            {
                // Captura otros errores de validación de negocio
                _logger.LogWarning(ex, "Validación de negocio fallida al crear reparación.");
                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Validación fallida",
                    ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la reparación.");
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error de creación",
                    "No se pudo crear la reparación."));
            }
        }

        /// <summary>
        /// Actualiza una Reparación existente.
        /// </summary>
        /// <param name="id">El ID único de la reparación a actualizar.</param>
        /// <param name="requestDto">DTO con los campos a modificar.</param>
        /// <returns>La Reparacion actualizada.</returns>
        /// <response code="200">Reparación actualizada exitosamente.</response>
        /// <response code="400">Datos de entrada o validación de negocio inválidos.</response>
        /// <response code="404">Reparación no encontrada.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ReparacionResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ActualizarReparacion(int id, [FromBody] ReparacionActualizacionRequestDto requestDto)
        {
            try

            {
                _logger.LogInformation("Actualizando reparación con ID: {Id}", id);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var (filas, reparaciondto) = await _reparacionService.ActualizarReparacionAsync(id, requestDto);

                if (filas == 0 && reparaciondto == null)
                {
                    throw new KeyNotFoundException($"No existe una reparación con ID {id} para actualizar.");
                }

                return Ok(reparaciondto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Reparación ID {Id} no encontrada para actualizar.", id);
                return NotFound(UtilidadesManejoErrores.CreateErrorResponse(
                   TipoError.ErrorNoEncontrado,
                   "No encontrado",
                   ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación de negocio fallida al actualizar reparación.");
                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Validación fallida",
                    ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar reparación con ID: {Id}", id);
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error de actualización",
                    "No se pudo actualizar la reparación."));
            }
        }
    }
}