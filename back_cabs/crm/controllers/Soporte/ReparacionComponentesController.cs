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

namespace back_cabs.CRM.Controllers.Soporte.Componentes
{
    ///<summary>
    /// Controlador para gestionar los componentes de las reparaciones.
    /// Proporciona endpoints para crear, actualizar y obtener componentes de reparación.
    /// </summary>
    ///<remarks>
    /// Rutas base: /api/soporte/reparaciones/componentes
    /// </remarks>
    [Route("api/soporte/reparaciones")] // La ruta base para el controlador de reparaciones
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

        // Este controlador se enfoca en los componentes de reparación.
        // Los endpoints para la entidad Reparacion principal (GET, POST, PUT)
        // se encuentran en ReparacionController.cs.
        // Este controlador solo expone los endpoints relacionados con ReparacionComponente.

        // --- ENDPOINTS (FUNCIONALIDADES DE COMPONENTES DE REPARACIÓN) ---
        /// <summary>
        /// Obtiene la lista completa de Componentes de Reparación.
        /// </summary>
        /// <returns>Lista de ReparacionComponenteResponseDto.</returns>
        /// <response code="200">Lista obtenida exitosamente.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// 
        [HttpGet("componentes")] // Ruta específica para componentes
        [ProducesResponseType(typeof(IEnumerable<ReparacionComponenteResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<List<ReparacionComponenteResponseDto>>> GetComponentesReparacion()
        {
            try
            {
                var componentes = await _reparacionService.ObtenerComponentesReparacionAsync();
                return Ok(componentes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de componentes de reparación.");
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error de lectura",
                    "No se pudieron obtener los componentes de reparación."));
            }
        }

        /// <summary>
        /// Obtiene un Componente de Reparación por su ID.
        /// </summary>
        /// <param name="id">El ID único del componente de reparación.</param>
        /// <returns>ReparacionComponenteResponseDto.</returns>
        /// <response code="200">Componente encontrado exitosamente.</response>
        /// <response code="404">Componente no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("componentes/{id}")]
        [ProducesResponseType(typeof(ReparacionComponenteResponseDto), (int)HttpStatusCode.OK)] // Ruta específica para componentes por ID
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<ReparacionComponenteResponseDto>> GetComponenteReparacionPorId(int id)
        {
            try
            {
                _logger.LogInformation("Buscando componente de reparación con ID: {Id}", id);

                var componente = await _reparacionService.ObtenerComponenteReparacionPorIdAsync(id);

                if (componente == null)
                {
                    _logger.LogWarning("Componente de reparación con ID {Id} no encontrado", id);
                    return NotFound(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorNoEncontrado,
                        "No encontrado",
                        $"No existe un componente de reparación con ID {id}"));
                }

                return Ok(componente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener componente de reparación con ID: {Id}", id);
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error de lectura",
                    "No se pudo obtener el componente de reparación."));
            }
        }

        /// <summary>
        /// Crea un nuevo Componente de Reparación.
        /// </summary>
        /// <param name="requestDto">DTO con los datos de creación del componente.</param>
        /// <returns>El componente recién creado.</returns>
        /// <response code="201">Componente creado exitosamente.</response>
        /// <response code="400">Datos de entrada inválidos (Validación o FKs).</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost("componentes")]
        [ProducesResponseType(typeof(ReparacionComponenteResponseDto), (int)HttpStatusCode.Created)] // Ruta específica para crear componentes
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<ReparacionComponenteResponseDto>> CrearComponenteReparacion([FromBody] ReparacionComponenteRequestDto requestDto)
        {
            try
            {
                _logger.LogInformation("Creando nuevo componente para la reparación ID: {ReparacionId}", requestDto.ReparacionId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var componenteCreado = await _reparacionService.CrearComponenteReparacionAsync(requestDto);

                _logger.LogInformation("Componente de reparación creado con ID: {Id}", componenteCreado.Id);

                // Devolvemos 201 Created con la ubicación del nuevo recurso
                return CreatedAtAction(nameof(GetComponenteReparacionPorId), new { id = componenteCreado.Id }, componenteCreado);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Validación de Clave Foránea fallida al crear componente.");
                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Referencia no encontrada",
                    ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el componente de reparación.");
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error de creación",
                    "No se pudo crear el componente de reparación."));
            }
        }

        /// <summary>
        /// Actualiza un Componente de Reparación existente.
        /// </summary>
        /// <param name="id">El ID único del componente a actualizar.</param>
        /// <param name="requestDto">DTO con los campos a modificar.</param>
        /// <returns>El componente actualizado.</returns>
        /// <response code="200">Componente actualizado exitosamente.</response>
        /// <response code="400">Datos de entrada inválidos.</response>
        /// <response code="404">Componente no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPut("componentes/{id}")] // Ruta específica para actualizar componentes por ID
        [ProducesResponseType(typeof(ReparacionComponenteResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ActualizarComponenteReparacion(int id, [FromBody] ReparacionComponenteActualizacionDto requestDto)
        {
            try
            {
                _logger.LogInformation("Actualizando componente de reparación con ID: {Id}", id);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var (filas, componenteDto) = await _reparacionService.ActualizarComponenteReparacionAsync(id, requestDto);

                if (componenteDto == null)
                {
                    // El servicio lanza KeyNotFoundException si no lo encuentra, así que este chequeo es una salvaguarda.
                    throw new KeyNotFoundException($"No existe un componente de reparación con ID {id} para actualizar.");
                }

                return Ok(componenteDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Componente de reparación ID {Id} no encontrado para actualizar.", id);
                return NotFound(UtilidadesManejoErrores.CreateErrorResponse(
                   TipoError.ErrorNoEncontrado,
                   "No encontrado",
                   ex.Message));
            }
            catch (ArgumentException ex) // Por si en el futuro agregas validaciones de negocio en el servicio
            {
                _logger.LogWarning(ex, "Validación de negocio fallida al actualizar componente.");
                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Validación fallida",
                    ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar componente de reparación con ID: {Id}", id);
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error de actualización",
                    "No se pudo actualizar el componente de reparación."));
            }
        }
    }
}
