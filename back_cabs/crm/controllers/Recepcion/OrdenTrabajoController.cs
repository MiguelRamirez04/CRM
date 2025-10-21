// =====================================================================================
// CONTROLADOR ORDEN TRABAJO - OrdenTrabajoController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los endpoints HTTP para operaciones de Órdenes de Trabajo en el módulo de Recepción.
// Incluye CRUD completo: crear, leer, actualizar órdenes de trabajo.
// Conectado con OrdenTrabajoService y base de datos ops.ordenes_trabajo.
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

using CRM.DTOs.Request;
using CRM.DTOs.Response;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.services;
using back_cabs.CRM.services.Recepcion;
using back_cabs.CRM.Middleware;
using back_cabs.CRM.enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Text.Json.Serialization;

namespace back_cabs.CRM.controllers.Recepcion
{
    /// <summary>
    /// Controlador para operaciones de Órdenes de Trabajo en el módulo de Recepción
    /// </summary>
    /// <remarks>
    /// Este controlador proporciona endpoints para la gestión completa de órdenes de trabajo:
    /// - GET /api/Recepcion - Lista de órdenes de trabajo con filtros opcionales
    /// - GET /api/Recepcion/{id} - Detalles de una orden específica
    /// - POST /api/Recepcion - Crear nueva orden (soporta clientes nuevos y legacy)
    /// - PUT /api/Recepcion/{id} - Actualizar orden existente
    /// - GET /api/Recepcion/estados - Lista de estados posibles
    /// - GET /api/Recepcion/clientes/buscar - Búsqueda de clientes para selección
    /// 
    /// Para crear una nueva orden con un cliente legacy:
    /// ```json
    /// {
    ///   "nuevoCliente": false,
    ///   "clienteId": 123,
    ///   "creadoPorUserId": 1,
    ///   "citaProgramadaInicio": "2023-11-30T10:00:00Z",
    ///   "modalidad": "Presencial",
    ///   "tipoOrden": "Asesoria",
    ///   "estado": "CAPTURADA"
    /// }
    /// ```
    /// 
    /// Para crear una nueva orden con un cliente nuevo:
    /// ```json
    /// {
    ///   "nuevoCliente": true,
    ///   "nombreCliente": "Cliente Nuevo XYZ",
    ///   "creadoPorUserId": 1,
    ///   "citaProgramadaInicio": "2023-11-30T10:00:00Z",
    ///   "modalidad": "Remoto",
    ///   "tipoOrden": "Cotizacion",
    ///   "estado": "CAPTURADA"
    /// }
    /// ```
    /// </remarks>
    public class OrdenTrabajoRequestWrapper
    {
        [JsonPropertyName("requestDto")]
        public OrdenTrabajoRequestDto? RequestDto { get; set; }
    }

    [ApiController]
    [Route("api/Recepcion")]
    [Produces("application/json")]
    [Authorize] // Todos los roles pueden acceder (ADMINISTRADOR, SOPORTE, RECEPCION)
    public class OrdenTrabajoController : ControllerBase
    {
        private readonly OrdenTrabajoService _ordenTrabajoService;
        private readonly ClientesCompletosService _clientesCompletosService;
        private readonly ILogger<OrdenTrabajoController> _logger;

        public OrdenTrabajoController(
            OrdenTrabajoService ordenTrabajoService,
            ClientesCompletosService clientesCompletosService,
            ILogger<OrdenTrabajoController> logger)
        {
            _ordenTrabajoService = ordenTrabajoService ?? throw new ArgumentNullException(nameof(ordenTrabajoService));
            _clientesCompletosService = clientesCompletosService ?? throw new ArgumentNullException(nameof(clientesCompletosService));
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
        /// <param name="estado">Filtrar por estado de orden (opcional)</param>
        /// <returns>Lista de OrdenTrabajoResponseDto</returns>
        /// <response code="200">Lista obtenida exitosamente</response>
        /// <response code="400">Estado inválido proporcionado</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// ```json
        /// [
        ///   {
        ///     "id": 1,
        ///     "nuevoCliente": false,
        ///     "clienteId": 123,
        ///     "nombreCliente": null,
        ///     "estado": "ASIGNADA",
        ///     "estadoDescripcion": "Orden asignada a un técnico",
        ///     "creadoPorUserId": 1,
        ///     "asignadaAUserId": 2,
        ///     "creadoEn": "2023-11-15T14:30:00Z"
        ///   },
        ///   {
        ///     "id": 2,
        ///     "nuevoCliente": true,
        ///     "clienteId": null,
        ///     "nombreCliente": "Cliente Nuevo XYZ",
        ///     "estado": "CAPTURADA",
        ///     "estadoDescripcion": "Orden capturada, pendiente de asignación",
        ///     "creadoPorUserId": 1,
        ///     "asignadaAUserId": null,
        ///     "creadoEn": "2023-11-15T15:45:00Z"
        ///   }
        /// ]
        /// ```
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrdenTrabajoResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IEnumerable<OrdenTrabajoResponseDto>>> GetAll(
            [FromQuery] int? skip = null, 
            [FromQuery] int? take = null,
            [FromQuery] string? estado = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo órdenes de trabajo. Estado={Estado}, Skip={Skip}, Take={Take}", 
                    estado ?? "TODOS", skip, take);

                // Validar el estado si se proporciona
                if (!string.IsNullOrEmpty(estado))
                {
                    try
                    {
                        _ = EstadoOrdenExtensions.FromDbValue(estado);
                    }
                    catch
                    {
                        return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                            TipoError.ErrorValidacion,
                            "Estado inválido",
                            $"El estado '{estado}' no es válido para filtrar órdenes."));
                    }
                }

                var ordenes = await _ordenTrabajoService.ObtenerTodasLasOrdenesAsync(skip, take, estado);

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
        // [GET] /api/Recepcion/test-data/{userId}/{clienteId}
        // ----------------------------------------------------------------------

        // ----------------------------------------------------------------------
        // [GET] /api/Recepcion/test-data/{userId}/{clienteId}
        // ----------------------------------------------------------------------

        /// <summary>
        /// Endpoint de prueba para verificar si usuario y cliente existen
        /// </summary>
        [HttpGet("test-data/{userId}/{clienteId}")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> TestData(int userId, int clienteId)
        {
            try
            {
                _logger.LogInformation("Probando existencia de usuario {UserId} y cliente {ClienteId}", userId, clienteId);

                // Verificar usuario directamente
                var usuarioExiste = await Task.Run(() => {
                    try {
                        return _ordenTrabajoService.GetType().GetMethod("VerificarUsuarioExisteAsync") != null;
                    } catch {
                        return false;
                    }
                });

                // Para simplificar, vamos a crear una orden de prueba mínima para ver si funciona
                var testDto = new OrdenTrabajoRequestDto {
                    NuevoCliente = false,
                    ClienteId = clienteId,
                    CitaProgramadaInicio = DateTime.Now.AddHours(1),
                    Modalidad = "Presencial",
                    TipoOrden = "Asesoria",
                    Estado = "CAPTURADA",
                    Prioridad = 3,
                    CreadoPorUserId = userId
                };

                try {
                    var orden = await _ordenTrabajoService.CrearOrdenTrabajoAsync(testDto);
                    return Ok(new {
                        success = true,
                        message = "Orden creada exitosamente",
                        ordenId = orden.Id,
                        usuario = new { id = userId, validado = true },
                        cliente = new { id = clienteId, validado = true }
                    });
                }
                catch (ArgumentException ex) {
                    return Ok(new {
                        success = false,
                        error = ex.Message,
                        usuario = new { id = userId, validado = ex.Message.Contains("Usuario") ? false : true },
                        cliente = new { id = clienteId, validado = ex.Message.Contains("Cliente") ? false : true }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al probar datos");
                return StatusCode(500, new { error = ex.Message });
            }
        }

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

                var orden = await _ordenTrabajoService.ObtenerOrdenPorIdAsync(id);

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
        /// <remarks>
        /// ## Modelo Híbrido de Clientes
        /// Este endpoint soporta un modelo híbrido para clientes:
        /// 
        /// **Opción 1: Cliente Legacy (existente)**
        /// - `nuevoCliente`: false
        /// - `clienteId`: ID del cliente existente (requerido)
        /// - `nombreCliente`: No es necesario (se ignora)
        /// 
        /// **Opción 2: Cliente Nuevo**
        /// - `nuevoCliente`: true
        /// - `clienteId`: No es necesario (se ignora)
        /// - `nombreCliente`: Nombre del nuevo cliente (requerido)
        /// 
        /// ## Estados Permitidos
        /// Utilice el endpoint GET /api/Recepcion/estados para obtener la lista de estados permitidos.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(OrdenTrabajoResponseDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<OrdenTrabajoResponseDto>> CrearOrden([FromBody] OrdenTrabajoRequestWrapper wrapper)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de orden de trabajo");

                // 1. Validar que el wrapper tenga el DTO
                if (wrapper?.RequestDto == null)
                {
                    _logger.LogWarning("Wrapper o requestDto es null");
                    return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorValidacion,
                        "Datos inválidos",
                        "El request debe contener un objeto 'requestDto' válido"));
                }

                var requestDto = wrapper.RequestDto;

                // 2. Validación automática del modelo (por DataAnnotations)
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Validación fallida al crear orden");
                    return BadRequest(ModelState);
                }
                
                // 2. Validación adicional para el cliente
                if (!requestDto.NuevoCliente && !requestDto.ClienteId.HasValue)
                {
                    _logger.LogWarning("Cliente legacy seleccionado sin proporcionar ID");
                    return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorValidacion,
                        "Cliente inválido",
                        "Para clientes existentes, debe proporcionar un ID de cliente válido"));
                }
                
                if (requestDto.NuevoCliente && string.IsNullOrWhiteSpace(requestDto.NombreCliente))
                {
                    _logger.LogWarning("Cliente nuevo seleccionado sin proporcionar nombre");
                    return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorValidacion,
                        "Cliente inválido",
                        "Para clientes nuevos, debe proporcionar un nombre de cliente"));
                }

                // 3. Crear orden usando el servicio
                var ordenCreada = await _ordenTrabajoService.CrearOrdenTrabajoAsync(requestDto);

                _logger.LogInformation("Orden creada exitosamente con ID: {Id}", ordenCreada.Id);

                // 4. Retorna 201 Created con la ubicación del nuevo recurso
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
        /// <response code="400">Datos de entrada inválidos</response>
        /// <response code="404">Orden no encontrada</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ActualizarOrden(int id, [FromBody] OrdenTrabajoUpdateRequestDto requestDto)
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

                // Validar el estado si se proporciona
                if (!string.IsNullOrEmpty(requestDto.Estado))
                {
                    try
                    {
                        _ = EstadoOrdenExtensions.FromDbValue(requestDto.Estado);
                    }
                    catch
                    {
                        return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                            TipoError.ErrorValidacion,
                            "Estado inválido",
                            $"El estado '{requestDto.Estado}' no es válido. Use uno de los estados permitidos."));
                    }
                }

                // Actualizar usando el servicio
                var actualizada = await _ordenTrabajoService.ActualizarOrdenTrabajoAsync(id, requestDto);

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
        /// Obtiene estadísticas detalladas del dashboard de recepción
        /// </summary>
        /// <returns>Estadísticas agregadas con desglose por estados</returns>
        /// <response code="200">Estadísticas obtenidas exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("estadisticas")]
        [ProducesResponseType(typeof(EstadisticasRecepcionResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<EstadisticasRecepcionResponseDto>> GetEstadisticas()
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas del dashboard");

                var estadisticas = await _ordenTrabajoService.ObtenerEstadisticasAsync();

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

        // ----------------------------------------------------------------------
        // [POST] /api/Recepcion/clientes/buscar
        // ----------------------------------------------------------------------

        /// <summary>
        /// Busca clientes por nombre o RFC para autocomplete
        /// </summary>
        /// <param name="busqueda">Texto de búsqueda</param>
        /// <param name="limite">Límite de resultados (opcional)</param>
        /// <returns>Lista de clientes resumidos</returns>
        /// <response code="200">Clientes encontrados</response>
        /// <response code="500">Error interno del servidor</response>
        /// <summary>
        /// Busca clientes legacy por nombre o RFC para autocompletado
        /// </summary>
        /// <param name="busqueda">Término de búsqueda (nombre o RFC)</param>
        /// <param name="limite">Número máximo de resultados (default: 10)</param>
        /// <returns>Lista de clientes resumidos</returns>
        /// <response code="200">Clientes encontrados exitosamente</response>
        /// <response code="400">Parámetros inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// ```json
        /// [
        ///   {
        ///     "clienteId": 42,
        ///     "nombreComercial": "Abarrotes Don Pepe S.A.",
        ///     "rfc": "ADP850101ABC",
        ///     "legacyClientId": 123
        ///   }
        /// ]
        /// ```
        /// </remarks>
        [HttpGet("clientes/buscar")]
        [Authorize] // Todos los roles pueden acceder a la búsqueda de clientes
        [ProducesResponseType(typeof(List<ClienteResumenDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<List<ClienteResumenDto>>> BuscarClientes(
            [FromQuery] string busqueda, 
            [FromQuery] int limite = 10)
        {
            try
            {
                _logger.LogInformation("Buscando clientes legacy con término: {Termino}", busqueda);

                if (string.IsNullOrWhiteSpace(busqueda))
                {
                    _logger.LogWarning("Búsqueda de clientes con texto vacío");
                    return Ok(new List<ClienteResumenDto>()); // Devolver lista vacía
                }

                if (limite <= 0 || limite > 50)
                {
                    return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorValidacion,
                        "Límite inválido",
                        "El límite debe estar entre 1 y 50"));
                }

                var clientes = await _ordenTrabajoService.BuscarClientesPorNombreORfcAsync(
                    busqueda, 
                    limite);

                _logger.LogInformation("Se encontraron {Count} clientes", clientes.Count);
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar clientes legacy");
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error en búsqueda de clientes",
                    "No se pudieron recuperar los clientes solicitados"));
            }
        }
        
        /// <summary>
        /// Obtiene la lista de clientes nuevos registrados en órdenes de trabajo
        /// </summary>
        /// <returns>Lista de clientes nuevos con nombre, teléfono y número de órdenes</returns>
        /// <response code="200">Clientes nuevos obtenidos exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// Este endpoint devuelve los clientes que fueron registrados como "nuevos" en las órdenes,
        /// es decir, clientes que no están en la base de datos legacy.
        /// 
        /// Ejemplo de respuesta:
        /// ```json
        /// [
        ///   {
        ///     "nombreCliente": "Abarrotes Don Pepe S.A.",
        ///     "telefono": 6182171064,
        ///     "numeroOrdenes": 3
        ///   },
        ///   {
        ///     "nombreCliente": "Taller Mecánico El Rayo",
        ///     "telefono": 6181234567,
        ///     "numeroOrdenes": 1
        ///   }
        /// ]
        /// ```
        /// </remarks>
        [HttpGet("clientes/nuevos")]
        [Authorize]
        [ProducesResponseType(typeof(List<ClienteNuevoDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<List<ClienteNuevoDto>>> ObtenerClientesNuevos()
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de clientes nuevos");
                
                var clientesNuevos = await _ordenTrabajoService.ObtenerClientesNuevosAsync();
                
                _logger.LogInformation("Se encontraron {Count} clientes nuevos únicos", clientesNuevos.Count);
                return Ok(clientesNuevos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes nuevos");
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error al obtener clientes nuevos",
                    "No se pudieron recuperar los clientes nuevos"));
            }
        }
        
        /// <summary>
        /// Obtiene la lista de estados posibles para una orden de trabajo
        /// </summary>
        /// <returns>Lista de estados con sus valores y descripciones</returns>
        /// <response code="200">Estados obtenidos exitosamente</response>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// ```json
        /// [
        ///   {
        ///     "id": 1,
        ///     "valor": "CAPTURADA",
        ///     "nombre": "CAPTURADA",
        ///     "descripcion": "Orden capturada, pendiente de asignación"
        ///   },
        ///   {
        ///     "id": 2,
        ///     "valor": "ASIGNADA",
        ///     "nombre": "ASIGNADA",
        ///     "descripcion": "Orden asignada a un técnico"
        ///   }
        /// ]
        /// ```
        /// </remarks>
        [HttpGet("estados")]
        [Authorize]
        [ProducesResponseType(typeof(List<object>), (int)HttpStatusCode.OK)]
        public ActionResult<List<object>> ObtenerEstados()
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de estados para órdenes de trabajo");
                
                // Obtener todos los valores del enum
                var estados = Enum.GetValues(typeof(EstadoOrden))
                    .Cast<EstadoOrden>()
                    .Select(e => new
                    {
                        Id = (int)e,
                        Valor = e.ToDbValue(),
                        Nombre = e.ToString(),
                        Descripcion = e.GetDescription()
                    })
                    .ToList();
                
                return Ok(estados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estados de órdenes");
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error al obtener estados",
                    "No se pudieron obtener los estados de las órdenes"));
            }
        }
    }
}