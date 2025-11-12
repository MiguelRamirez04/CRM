// =====================================================================================
// CONTROLADOR AGENTE - AgenteController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Controlador REST API para operaciones de agentes.
// Expone endpoints GET para consultar agentes del catálogo.
//
// ENDPOINTS:
// - GET /api/Agente → Obtener todos los agentes activos
// - GET /api/Agente/{id} → Obtener agente por ID
// - GET /api/Agente/{id}/exists → Validar existencia de agente
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
    /// Controlador para operaciones CRUD de agentes
    /// Maneja las solicitudes HTTP y delega lógica de negocio al servicio
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AgenteController : ControllerBase
    {
        private readonly IAgenteService _service;
        private readonly ILogger<AgenteController> _logger;

        public AgenteController(
            IAgenteService service,
            ILogger<AgenteController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ---------------------------------------------------------------------
        // 📖 ENDPOINTS DE LECTURA
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todos los agentes activos del catálogo
        /// </summary>
        /// <returns>Lista de agentes activos</returns>
        /// <response code="200">Lista de agentes obtenida exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AgenteResponseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllAgentes()
        {
            try
            {
                var agentes = await _service.GetAllAgentesAsync();
                _logger.LogInformation("Solicitud GET /api/Agente - Retornando {Count} agentes", agentes.Count());
                return Ok(agentes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los agentes en controlador");
                return StatusCode(500, new { error = "Error interno al obtener agentes" });
            }
        }

        /// <summary>
        /// Obtiene un agente específico por su ID
        /// </summary>
        /// <param name="id">ID del agente</param>
        /// <returns>Agente encontrado o 404 si no existe</returns>
        /// <response code="200">Agente encontrado</response>
        /// <response code="404">Agente no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AgenteResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAgenteById(int id)
        {
            try
            {
                var agente = await _service.GetAgenteByIdAsync(id);

                if (agente == null)
                {
                    _logger.LogWarning("Agente no encontrado con ID: {Id}", id);
                    return NotFound(new { error = $"Agente con ID {id} no encontrado" });
                }

                _logger.LogDebug("Agente encontrado: {NombreAgente} (ID: {Id})", agente.NombreAgente, agente.Id);
                return Ok(agente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agente por ID {Id} en controlador", id);
                return StatusCode(500, new { error = "Error interno al obtener agente" });
            }
        }

        // ---------------------------------------------------------------------
        // 🔍 ENDPOINTS DE VALIDACIÓN
        // ---------------------------------------------------------------------

        /// <summary>
        /// Valida si existe un agente con el ID especificado
        /// </summary>
        /// <param name="id">ID del agente a validar</param>
        /// <returns>true si existe, false en caso contrario</returns>
        /// <response code="200">Resultado de validación</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id:int}/exists")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ValidateAgenteExists(int id)
        {
            try
            {
                var exists = await _service.ValidateAgenteExistsAsync(id);
                _logger.LogDebug("Validación de existencia para agente ID {Id}: {Exists}", id, exists);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar existencia de agente ID {Id} en controlador", id);
                return StatusCode(500, new { error = "Error interno al validar agente" });
            }
        }

        // ---------------------------------------------------------------------
        // ✏️ ENDPOINTS DE ESCRITURA - Futuras implementaciones
        // ---------------------------------------------------------------------

        // Endpoints para operaciones de escritura se agregarían aquí cuando sean necesarias
        // - POST /api/Agente
        // - PUT /api/Agente/{id}
        // - DELETE /api/Agente/{id}
    }
}