// =====================================================================================
// CONTROLADOR RECEPCION - RecepcionController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los endpoints HTTP para operaciones de Órdenes de Trabajo en el módulo de Recepción.
// Incluye CRUD completo: crear, leer, actualizar órdenes de trabajo.
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
using back_cabs.CRM.enums;
using back_cabs.CRM.models.Recepcion;
using back_cabs.middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    public class RecepcionController : ControllerBase
    {
        private readonly ILogger<RecepcionController> _logger;

        public RecepcionController(ILogger<RecepcionController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Datos de prueba en memoria (reemplazar con DbContext en producción)
        private static List<OrdenTrabajo> _ordenes = new List<OrdenTrabajo>
        {
            new OrdenTrabajo
            {
                Id = 1,
                Notas = "Orden de prueba inicial",
                CitaProgramadaInicio = DateTime.UtcNow.AddDays(1),
                CitaProgramadaFin = DateTime.UtcNow.AddDays(1).AddHours(2),
                Modalidad = "Presencial",
                TipoOrden = "Asesoria",
                LegacyClientId = 1,
                Prioridad = 3,
                Estado = true,
                UbicacionText = "Oficina principal",
                CostoEstimado = 500,
                id_usuario = 1,
                CreadoEn = DateTime.UtcNow,
                ActualizadoEn = DateTime.UtcNow
            }
        };
        private static int _nextId = 2;

        // ----------------------------------------------------------------------
        // [GET] /api/Recepcion
        // ----------------------------------------------------------------------

        /// <summary>
        /// Obtiene la lista completa de Órdenes de Trabajo
        /// </summary>
        /// <returns>Lista de OrdenTrabajoResponseDto</returns>
        /// <response code="200">Lista obtenida exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrdenTrabajoResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public ActionResult<IEnumerable<OrdenTrabajoResponseDto>> GetAll()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las órdenes de trabajo. Total: {Count}", _ordenes.Count);

                // Mapeo (modelo dominio -> DTO de respuesta)
                var responseDtos = _ordenes.Select(o => new OrdenTrabajoResponseDto
                {
                    Id = o.Id,
                    Notas = o.Notas,
                    CitaProgramadaInicio = o.CitaProgramadaInicio,
                    CitaProgramadaFin = o.CitaProgramadaFin,
                    Modalidad = o.Modalidad,
                    TipoOrden = o.TipoOrden,
                    LegacyClientId = o.LegacyClientId ?? 0,
                    Prioridad = o.Prioridad,
                    Estado = o.Estado,
                    UbicacionText = o.UbicacionText,
                    EstadoFacturado = !string.IsNullOrEmpty(o.EstadoFacturado) ? bool.Parse(o.EstadoFacturado) : (bool?)null,
                    RequiereFactura = o.RequiereFactura,
                    CostoReal = o.CostoReal,
                    CostoEstimado = o.CostoEstimado,
                    CreadoEn = o.CreadoEn,
                    ActualizadoEn = o.ActualizadoEn,
                    id_usuario = o.id_usuario
                }).ToList();

                return Ok(responseDtos);
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
        public ActionResult<OrdenTrabajoResponseDto> CrearOrden([FromBody] OrdenTrabajoCreacionRequestDto requestDto)
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

                // 2. Mapeo (DTO de Creación -> Modelo de Dominio)
                var nuevaOrden = new OrdenTrabajo
                {
                    Id = _nextId++,
                    Notas = requestDto.Notas,
                    CitaProgramadaInicio = requestDto.CitaProgramadaInicio,
                    CitaProgramadaFin = requestDto.CitaProgramadaFin,
                    Modalidad = requestDto.Modalidad,
                    TipoOrden = requestDto.TipoOrden,
                    Cotizaciones = requestDto.Cotizaciones,
                    LegacyClientId = requestDto.LegacyClientId,
                    Prioridad = requestDto.Prioridad,
                    Estado = requestDto.Estado,
                    UbicacionText = requestDto.UbicacionText,
                    EstadoFacturado = requestDto.EstadoFacturado != null ? requestDto.EstadoFacturado.ToString() : null,
                    RequiereFactura = requestDto.RequiereFactura,
                    FacturaFolio = requestDto.FacturaFolio,
                    CostoReal = requestDto.CostoReal,
                    CostoEstimado = requestDto.CostoEstimado,
                    id_usuario = requestDto.id_usuario,
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                // 3. Guardar en la "base de datos"
                _ordenes.Add(nuevaOrden);

                _logger.LogInformation("Orden creada exitosamente con ID: {Id}", nuevaOrden.Id);

                // 4. Mapeo final (Modelo de Dominio -> DTO de Respuesta)
                var responseDto = new OrdenTrabajoResponseDto
                {
                    Id = nuevaOrden.Id,
                    Notas = nuevaOrden.Notas,
                    CitaProgramadaInicio = nuevaOrden.CitaProgramadaInicio,
                    CitaProgramadaFin = nuevaOrden.CitaProgramadaFin,
                    Modalidad = nuevaOrden.Modalidad,
                    TipoOrden = nuevaOrden.TipoOrden,
                    LegacyClientId = nuevaOrden.LegacyClientId ?? 0,
                    Prioridad = nuevaOrden.Prioridad,
                    Estado = nuevaOrden.Estado,
                    UbicacionText = nuevaOrden.UbicacionText,
                    EstadoFacturado = !string.IsNullOrEmpty(nuevaOrden.EstadoFacturado) ? bool.Parse(nuevaOrden.EstadoFacturado) : null,
                    RequiereFactura = nuevaOrden.RequiereFactura,
                    CostoReal = nuevaOrden.CostoReal,
                    CostoEstimado = nuevaOrden.CostoEstimado,
                    CreadoEn = nuevaOrden.CreadoEn,
                    ActualizadoEn = nuevaOrden.ActualizadoEn,
                    id_usuario = nuevaOrden.id_usuario
                };

                // 5. Retorna 201 Created con la ubicación del nuevo recurso
                return CreatedAtAction(nameof(GetOrdenPorId), new { id = responseDto.Id }, responseDto);
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
        public ActionResult<OrdenTrabajoResponseDto> GetOrdenPorId(int id)
        {
            try
            {
                _logger.LogInformation("Buscando orden de trabajo con ID: {Id}", id);

                // 1. Buscar el modelo
                var orden = _ordenes.FirstOrDefault(o => o.Id == id);

                if (orden == null)
                {
                    _logger.LogWarning("Orden de trabajo con ID {Id} no encontrada", id);
                    return NotFound(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorNoEncontrado,
                        "Orden no encontrada",
                        $"No existe una orden de trabajo con ID {id}"));
                }

                // 2. Mapeo (Modelo de Dominio -> DTO de Respuesta)
                var responseDto = new OrdenTrabajoResponseDto
                {
                    Id = orden.Id,
                    Notas = orden.Notas,
                    CitaProgramadaInicio = orden.CitaProgramadaInicio,
                    CitaProgramadaFin = orden.CitaProgramadaFin,
                    Modalidad = orden.Modalidad,
                    TipoOrden = orden.TipoOrden,
                    LegacyClientId = orden.LegacyClientId ?? 0,
                    Prioridad = orden.Prioridad,
                    Estado = orden.Estado,
                    UbicacionText = orden.UbicacionText,
                    EstadoFacturado = !string.IsNullOrEmpty(orden.EstadoFacturado) ? bool.Parse(orden.EstadoFacturado) : null,
                    RequiereFactura = orden.RequiereFactura,
                    CostoReal = orden.CostoReal,
                    CostoEstimado = orden.CostoEstimado,
                    CreadoEn = orden.CreadoEn,
                    ActualizadoEn = orden.ActualizadoEn,
                    id_usuario = orden.id_usuario
                };

                _logger.LogInformation("Orden encontrada exitosamente: ID {Id}", id);
                return Ok(responseDto);
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
        public IActionResult ActualizarOrden(int id, [FromBody] OrdenTrabajoActualizacionRequestDto requestDto)
        {
            try
            {
                _logger.LogInformation("Actualizando orden de trabajo con ID: {Id}", id);

                // 1. Buscar el modelo
                var ordenExistente = _ordenes.FirstOrDefault(o => o.Id == id);

                if (ordenExistente == null)
                {
                    _logger.LogWarning("Orden de trabajo con ID {Id} no encontrada para actualizar", id);
                    return NotFound(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorValidacion,
                        "Orden no encontrada",
                        $"No existe una orden de trabajo con ID {id} para actualizar"));
                }

                // 2. Aplicar actualizaciones solo si el campo viene en el DTO
                if (requestDto.Notas is not null) ordenExistente.Notas = requestDto.Notas;
                if (requestDto.CitaProgramadaInicio is not null) ordenExistente.CitaProgramadaInicio = requestDto.CitaProgramadaInicio.Value;
                if (requestDto.CitaProgramadaFin is not null) ordenExistente.CitaProgramadaFin = requestDto.CitaProgramadaFin;
                if (requestDto.Prioridad is not null) ordenExistente.Prioridad = requestDto.Prioridad.Value;
                if (requestDto.Estado is not null) ordenExistente.Estado = requestDto.Estado.Value;
                if (requestDto.EstadoFacturado is not null) ordenExistente.EstadoFacturado = requestDto.EstadoFacturado.Value.ToString();
                if (requestDto.RequiereFactura is not null) ordenExistente.RequiereFactura = requestDto.RequiereFactura.Value;
                if (requestDto.FacturaFolio is not null) ordenExistente.FacturaFolio = requestDto.FacturaFolio;
                if (requestDto.CostoReal is not null) ordenExistente.CostoReal = requestDto.CostoReal;
                if (requestDto.CostoEstimado is not null) ordenExistente.CostoEstimado = requestDto.CostoEstimado;
                if (requestDto.id_usuario != 0) ordenExistente.id_usuario = requestDto.id_usuario;

                // 3. Actualizar timestamp
                ordenExistente.ActualizadoEn = DateTime.UtcNow;

                _logger.LogInformation("Orden actualizada exitosamente: ID {Id}", id);

                // 4. Retorna 204 No Content
                return NoContent();
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
    }
}