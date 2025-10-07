// =====================================================================================
// CONTROLADOR RECEPCION - RecepcionController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los endpoints HTTP para operaciones de Órdenes de Trabajo en el módulo de Recepción.
// Incluye CRUD completo: crear, leer, actualizar órdenes de trabajo.
// Conectado con DashRecepcionService y base de datos ops.ordenes_trabajo.
//
// CUÁNDO USARLO:
// - Gestión de órdenes de trabajo (GET, POST, PUT)
// - Asignación de asesorías a clientes
// - Seguimiento de estado de órdenes
//
// CÓMO USARLO:
// Los endpoints se exponen automáticamente en:
// GET    /api/Recepcion           - Obtener todas las órdenes
// GET    /api/Recepcion/{id}      - Obtener orden por ID
// POST   /api/Recepcion           - Crear nueva orden
// PUT    /api/Recepcion/{id}      - Actualizar orden existente
//
// =====================================================================================

using back_cabs.CRM.DTOs.Recepcion;
using back_cabs.CRM.services.Recepcion;
using back_cabs.CRM.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace back_cabs.CRM.controllers.Recepcion
{
    /// <summary>
    /// Controlador para operaciones de Órdenes de Trabajo en el módulo de Recepción
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // Todos los roles pueden acceder (ADMINISTRADOR, SOPORTE, RECEPCION)
    public class RecepcionController : ControllerBase
    {
        private readonly DashRecepcionService _recepcionService;
        private readonly ILogger<RecepcionController> _logger;

        public RecepcionController(
            DashRecepcionService recepcionService,
            ILogger<RecepcionController> logger)
        {
            _recepcionService = recepcionService ?? throw new ArgumentNullException(nameof(recepcionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ----------------------------------------------------------------------
        // [GET] /api/Recepcion
        // ----------------------------------------------------------------------

        /// <summary>
        /// Obtiene la lista completa de Órdenes de Trabajo
        /// </summary>
        /// <param name="skip">Número de registros a saltar (paginación)</param>
        /// <param name="take">Número de registros a obtener (paginación)</param>
        /// <returns>Lista de OrdenTrabajoResponseDto</returns>
        /// <response code="200">Lista obtenida exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrdenTrabajoResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IEnumerable<OrdenTrabajoResponseDto>>> GetAll([FromQuery] int? skip = null, [FromQuery] int? take = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las órdenes de trabajo");

                var ordenes = await _recepcionService.ObtenerTodasLasOrdenesAsync(skip, take);

                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes de trabajo");
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error al obtener órdenes",
                    "No se pudieron cargar las órdenes de trabajo"));
            }
        }

        // ----------------------------------------------------------------------
        // [GET] /api/Recepcion/{id}
        // ----------------------------------------------------------------------

        /// <summary>
        /// Obtiene una Orden de Trabajo por su ID
        /// </summary>
        /// <param name="id">El ID único de la orden</param>
        /// <returns>OrdenTrabajoResponseDto</returns>
        /// <response code="200">Orden encontrada exitosamente</response>
        /// <response code="404">Orden no encontrada</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrdenTrabajoResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<OrdenTrabajoResponseDto>> GetOrdenPorId(int id)
        {
            try
            {
                _logger.LogInformation("Buscando orden de trabajo con ID: {Id}", id);

                var orden = await _recepcionService.ObtenerOrdenPorIdAsync(id);

                if (orden == null)
                {
                    _logger.LogWarning("Orden de trabajo con ID {Id} no encontrada", id);
                    return NotFound(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorNoEncontrado,
                        "Orden no encontrada",
                        $"No existe una orden de trabajo con ID {id}"));
                }

                _logger.LogInformation("Orden encontrada exitosamente: ID {Id}", id);
                return Ok(orden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener orden de trabajo con ID: {Id}", id);
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error al obtener orden",
                    "No se pudo obtener la orden de trabajo"));
            }
        }

        // ----------------------------------------------------------------------
        // [POST] /api/Recepcion
        // ----------------------------------------------------------------------

        /// <summary>
        /// Crea una nueva Orden de Trabajo
        /// </summary>
        /// <param name="requestDto">DTO con los datos de creación</param>
        /// <returns>La Orden de Trabajo recién creada</returns>
        /// <response code="201">Orden creada exitosamente</response>
        /// <response code="400">Datos de entrada inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrdenTrabajoResponseDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<OrdenTrabajoResponseDto>> CrearOrden([FromBody] OrdenTrabajoCreacionRequestDto requestDto)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de orden de trabajo");

                // 1. Validación automática del modelo (por DataAnnotations)
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Validación fallida al crear orden");
                    return BadRequest(ModelState);
                }

                // 2. Crear orden usando el servicio
                var ordenCreada = await _recepcionService.CrearOrdenTrabajoAsync(requestDto);

                _logger.LogInformation("Orden creada exitosamente con ID: {Id}", ordenCreada.Id);

                // 3. Retorna 201 Created con la ubicación del nuevo recurso
                return CreatedAtAction(nameof(GetOrdenPorId), new { id = ordenCreada.Id }, ordenCreada);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación de negocio fallida al crear orden");
                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Validación fallida",
                    ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida al crear orden");
                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Operación inválida",
                    ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear orden de trabajo");
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error al crear orden",
                    "No se pudo crear la orden de trabajo"));
            }
        }

        // ----------------------------------------------------------------------
        // [PUT] /api/Recepcion/{id}
        // ----------------------------------------------------------------------

        /// <summary>
        /// Actualiza una Orden de Trabajo existente
        /// </summary>
        /// <param name="id">El ID único de la orden a actualizar</param>
        /// <param name="requestDto">DTO con los campos a modificar (anulables)</param>
        /// <returns>No Content si la actualización es exitosa</returns>
        /// <response code="204">Orden actualizada exitosamente</response>
        /// <response code="404">Orden no encontrada</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ActualizarOrden(int id, [FromBody] OrdenTrabajoActualizacionRequestDto requestDto)
        {
            try
            {
                _logger.LogInformation("Actualizando orden de trabajo con ID: {Id}", id);

                // Validación automática del modelo
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Validación fallida al actualizar orden");
                    return BadRequest(ModelState);
                }

                // Actualizar usando el servicio
                var actualizada = await _recepcionService.ActualizarOrdenTrabajoAsync(id, requestDto);

                if (!actualizada)
                {
                    _logger.LogWarning("Orden de trabajo con ID {Id} no encontrada para actualizar", id);
                    return NotFound(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorNoEncontrado,
                        "Orden no encontrada",
                        $"No existe una orden de trabajo con ID {id} para actualizar"));
                }

                _logger.LogInformation("Orden actualizada exitosamente: ID {Id}", id);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación de negocio fallida al actualizar orden");
                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Validación fallida",
                    ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar orden de trabajo con ID: {Id}", id);
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error al actualizar orden",
                    "No se pudo actualizar la orden de trabajo"));
            }
        }

        // ----------------------------------------------------------------------
        // [GET] /api/Recepcion/estadisticas
        // ----------------------------------------------------------------------

        /// <summary>
        /// Obtiene estadísticas del dashboard de recepción
        /// </summary>
        /// <returns>Estadísticas agregadas</returns>
        /// <response code="200">Estadísticas obtenidas exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("estadisticas")]
        [ProducesResponseType(typeof(Dictionary<string, object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<Dictionary<string, object>>> GetEstadisticas()
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas del dashboard");

                var estadisticas = await _recepcionService.ObtenerEstadisticasAsync();

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error al obtener estadísticas",
                    "No se pudieron cargar las estadísticas del dashboard"));
            }
        }
    }
}