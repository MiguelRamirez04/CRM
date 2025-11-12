// =====================================================================================
// CONTROLADOR ALMACEN - AlmacenController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Controlador REST API para operaciones de almacenes.
// Expone endpoints GET para consultar almacenes del catálogo.
//
// ENDPOINTS:
// - GET /api/Almacen → Obtener todos los almacenes activos
// - GET /api/Almacen/{id} → Obtener almacen por ID
//
// AUTORIZACIÓN:
// - Requiere roles: ADMINISTRACION, RECEPCION (igual que otros catálogos)
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Interfaces.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_cabs.CRM.controllers
{
    /// <summary>
    /// Controlador para operaciones CRUD de almacenes
    /// Maneja las solicitudes HTTP y delega lógica de negocio al servicio
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AlmacenController : ControllerBase
    {
        private readonly IAlmacenService _service;
        private readonly ILogger<AlmacenController> _logger;

        public AlmacenController(
            IAlmacenService service,
            ILogger<AlmacenController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ---------------------------------------------------------------------
        // 📖 ENDPOINTS DE LECTURA
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todos los almacenes activos del catálogo
        /// </summary>
        /// <returns>Lista de almacenes activos</returns>
        /// <response code="200">Lista de almacenes obtenida exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AlmacenResponseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllAlmacenes()
        {
            try
            {
                var almacenes = await _service.GetAllAlmacenesAsync();
                _logger.LogInformation("Solicitud GET /api/Almacen - Retornando {Count} almacenes", almacenes.Count());
                return Ok(almacenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los almacenes en controlador");
                return StatusCode(500, new { error = "Error interno al obtener almacenes" });
            }
        }

        /// <summary>
        /// Obtiene un almacen específico por su ID
        /// </summary>
        /// <param name="id">ID del almacen</param>
        /// <returns>Almacen encontrado o 404 si no existe</returns>
        /// <response code="200">Almacen encontrado</response>
        /// <response code="404">Almacen no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AlmacenResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAlmacenById(int id)
        {
            try
            {
                var almacen = await _service.GetAlmacenByIdAsync(id);

                if (almacen == null)
                {
                    _logger.LogWarning("Almacen no encontrado con ID: {Id}", id);
                    return NotFound(new { error = $"Almacen con ID {id} no encontrado" });
                }

                _logger.LogDebug("Almacen encontrado: {NombreAlmacen} (ID: {Id})", almacen.NombreAlmacen, almacen.Id);
                return Ok(almacen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener almacen por ID {Id} en controlador", id);
                return StatusCode(500, new { error = "Error interno al obtener almacen" });
            }
        }

        // ---------------------------------------------------------------------
        // 🔍 ENDPOINTS DE VALIDACIÓN
        // ---------------------------------------------------------------------

        /// <summary>
        /// Valida si existe un almacen con el ID especificado
        /// </summary>
        /// <param name="id">ID del almacen a validar</param>
        /// <returns>true si existe, false en caso contrario</returns>
        /// <response code="200">Resultado de validación</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id:int}/exists")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ValidateAlmacenExists(int id)
        {
            try
            {
                var exists = await _service.ValidateAlmacenExistsAsync(id);
                _logger.LogDebug("Validación de existencia para almacen ID {Id}: {Exists}", id, exists);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar existencia de almacen ID {Id} en controlador", id);
                return StatusCode(500, new { error = "Error interno al validar almacen" });
            }
        }

        // ---------------------------------------------------------------------
        // ✏️ ENDPOINTS DE ESCRITURA - Futuras implementaciones
        // ---------------------------------------------------------------------

        // Endpoints para operaciones de escritura se agregarían aquí cuando sean necesarias
        // - POST /api/Almacen
        // - PUT /api/Almacen/{id}
        // - DELETE /api/Almacen/{id}
    }
}