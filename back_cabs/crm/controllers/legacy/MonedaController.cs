// =====================================================================================
// CONTROLADOR MONEDA - MonedaController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Controlador REST API para operaciones de monedas.
// Expone endpoints GET para consultar monedas del catálogo.
//
// ENDPOINTS:
// - GET /api/Moneda → Obtener todas las monedas activas
// - GET /api/Moneda/{id} → Obtener moneda por ID
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
    /// Controlador para operaciones CRUD de monedas
    /// Maneja las solicitudes HTTP y delega lógica de negocio al servicio
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class MonedaController : ControllerBase
    {
        private readonly IMonedaService _service;
        private readonly ILogger<MonedaController> _logger;

        public MonedaController(
            IMonedaService service,
            ILogger<MonedaController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ---------------------------------------------------------------------
        // 📖 ENDPOINTS DE LECTURA
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todas las monedas activas del catálogo
        /// </summary>
        /// <returns>Lista de monedas activas</returns>
        /// <response code="200">Lista de monedas obtenida exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MonedaResponseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllMonedas()
        {
            try
            {
                var monedas = await _service.GetAllMonedasAsync();
                _logger.LogInformation("Solicitud GET /api/Moneda - Retornando {Count} monedas", monedas.Count());
                return Ok(monedas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las monedas en controlador");
                return StatusCode(500, new { error = "Error interno al obtener monedas" });
            }
        }

        /// <summary>
        /// Obtiene una moneda específica por su ID
        /// </summary>
        /// <param name="id">ID de la moneda</param>
        /// <returns>Moneda encontrada o 404 si no existe</returns>
        /// <response code="200">Moneda encontrada</response>
        /// <response code="404">Moneda no encontrada</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MonedaResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetMonedaById(int id)
        {
            try
            {
                var moneda = await _service.GetMonedaByIdAsync(id);

                if (moneda == null)
                {
                    _logger.LogWarning("Moneda no encontrada con ID: {Id}", id);
                    return NotFound(new { error = $"Moneda con ID {id} no encontrada" });
                }

                _logger.LogDebug("Moneda encontrada: {NombreMoneda} (ID: {Id})", moneda.NombreMoneda, moneda.Id);
                return Ok(moneda);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener moneda por ID {Id} en controlador", id);
                return StatusCode(500, new { error = "Error interno al obtener moneda" });
            }
        }

        // ---------------------------------------------------------------------
        // 🔍 ENDPOINTS DE VALIDACIÓN
        // ---------------------------------------------------------------------

        /// <summary>
        /// Valida si existe una moneda con el ID especificado
        /// </summary>
        /// <param name="id">ID de la moneda a validar</param>
        /// <returns>true si existe, false en caso contrario</returns>
        /// <response code="200">Resultado de validación</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id:int}/exists")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ValidateMonedaExists(int id)
        {
            try
            {
                var exists = await _service.ValidateMonedaExistsAsync(id);
                _logger.LogDebug("Validación de existencia para moneda ID {Id}: {Exists}", id, exists);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar existencia de moneda ID {Id} en controlador", id);
                return StatusCode(500, new { error = "Error interno al validar moneda" });
            }
        }

        // ---------------------------------------------------------------------
        // ✏️ ENDPOINTS DE ESCRITURA - Futuras implementaciones
        // ---------------------------------------------------------------------

        // Endpoints para operaciones de escritura se agregarían aquí cuando sean necesarias
        // - POST /api/Moneda
        // - PUT /api/Moneda/{id}
        // - DELETE /api/Moneda/{id}
    }
}