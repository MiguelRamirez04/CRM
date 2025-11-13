// =====================================================================================
// CONCEPTO CONTROLLER - ConceptoController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Expone endpoints REST para conceptos con métricas de performance.
// Implementa GET APIs con cache Redis integrado.
//
// ENDPOINTS:
// - GET /api/Concepto → Lista todos los conceptos activos
// - GET /api/Concepto/search?q={term} → Busca por código o nombre
//
// AUTENTICACIÓN:
// - Requiere JWT token válido
// - Roles permitidos: ADMINISTRACION, RECEPCION
//
// PERFORMANCE:
// - Cache Redis integrado (30min para GetAll, 60min para Search)
// - Logging detallado de tiempos de respuesta
// - Manejo de errores con códigos HTTP apropiados
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Interfaces.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace back_cabs.CRM.controllers
{
    /// <summary>
    /// Controlador REST para conceptos
    /// Expone APIs GET con cache Redis y métricas de performance
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class ConceptoController : ControllerBase
    {
        private readonly IConceptoService _service;
        private readonly ILogger<ConceptoController> _logger;

        public ConceptoController(
            IConceptoService service,
            ILogger<ConceptoController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ─────────────────────────────────────────────────────────────────
        // 📖 ENDPOINTS GET
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todos los conceptos activos
        /// </summary>
        /// <returns>Lista de conceptos con cache Redis</returns>
        /// <response code="200">Lista de conceptos retornada exitosamente</response>
        /// <response code="401">No autorizado - Token JWT inválido</response>
        /// <response code="403">Prohibido - Rol no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ConceptoResponseDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllConceptos()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📋 Solicitud GET /api/Concepto iniciada");

                var conceptos = await _service.GetAllConceptosAsync();

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/Concepto completado en {ElapsedMs}ms. Retornando {Count} conceptos",
                    stopwatch.ElapsedMilliseconds, conceptos.Count());

                return Ok(conceptos);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/Concepto después de {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "No se pudieron obtener los conceptos",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Busca conceptos por término de búsqueda
        /// </summary>
        /// <param name="q">Término de búsqueda (se busca en código y nombre)</param>
        /// <returns>Lista de conceptos que coinciden con el criterio</returns>
        /// <response code="200">Búsqueda completada exitosamente</response>
        /// <response code="400">Parámetro de búsqueda inválido</response>
        /// <response code="401">No autorizado - Token JWT inválido</response>
        /// <response code="403">Prohibido - Rol no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<ConceptoResponseDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchConceptos([FromQuery] string q)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Validar parámetro de búsqueda
                if (string.IsNullOrWhiteSpace(q))
                {
                    _logger.LogWarning("⚠️ Parámetro 'q' vacío en búsqueda de conceptos");
                    return BadRequest(new
                    {
                        error = "Parámetro requerido",
                        message = "El parámetro 'q' es obligatorio para la búsqueda",
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("🔍 Solicitud GET /api/Concepto/search?q={SearchTerm} iniciada", q);

                var conceptos = await _service.SearchConceptosAsync(q);

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/Concepto/search?q={SearchTerm} completado en {ElapsedMs}ms. Retornando {Count} conceptos",
                    q, stopwatch.ElapsedMilliseconds, conceptos.Count());

                return Ok(conceptos);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/Concepto/search?q={SearchTerm} después de {ElapsedMs}ms", q, stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "No se pudo realizar la búsqueda de conceptos",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
