// =====================================================================================
// DOCUMENTO MODELO CONTROLLER - DocumentoModeloController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Expone endpoints REST para documentos modelo con métricas de performance.
// Implementa GET APIs con cache Redis integrado.
//
// ENDPOINTS:
// - GET /api/DocumentoModelo → Lista todos los documentos modelo activos
// - GET /api/DocumentoModelo/search?q={term} → Busca por descripción
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
    /// Controlador REST para documentos modelo
    /// Expone APIs GET con cache Redis y métricas de performance
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class DocumentoModeloController : ControllerBase
    {
        private readonly IDocumentoModeloService _service;
        private readonly ILogger<DocumentoModeloController> _logger;

        public DocumentoModeloController(
            IDocumentoModeloService service,
            ILogger<DocumentoModeloController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ─────────────────────────────────────────────────────────────────
        // 📖 ENDPOINTS GET
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todos los documentos modelo activos
        /// </summary>
        /// <returns>Lista de documentos modelo con cache Redis</returns>
        /// <response code="200">Lista de documentos modelo retornada exitosamente</response>
        /// <response code="401">No autorizado - Token JWT inválido</response>
        /// <response code="403">Prohibido - Rol no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DocumentoModeloResponseDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllDocumentoModelos()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📋 Solicitud GET /api/DocumentoModelo iniciada");

                var documentosModelo = await _service.GetAllDocumentoModelosAsync();

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/DocumentoModelo completado en {ElapsedMs}ms. Retornando {Count} documentos modelo",
                    stopwatch.ElapsedMilliseconds, documentosModelo.Count());

                return Ok(documentosModelo);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/DocumentoModelo después de {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "No se pudieron obtener los documentos modelo",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Busca documentos modelo por término de búsqueda
        /// </summary>
        /// <param name="q">Término de búsqueda (se busca en descripción)</param>
        /// <returns>Lista de documentos modelo que coinciden con el criterio</returns>
        /// <response code="200">Búsqueda completada exitosamente</response>
        /// <response code="400">Parámetro de búsqueda inválido</response>
        /// <response code="401">No autorizado - Token JWT inválido</response>
        /// <response code="403">Prohibido - Rol no autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<DocumentoModeloResponseDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchDocumentoModelos([FromQuery] string q)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Validar parámetro de búsqueda
                if (string.IsNullOrWhiteSpace(q))
                {
                    _logger.LogWarning("⚠️ Parámetro 'q' vacío en búsqueda de documentos modelo");
                    return BadRequest(new
                    {
                        error = "Parámetro requerido",
                        message = "El parámetro 'q' es obligatorio para la búsqueda",
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("🔍 Solicitud GET /api/DocumentoModelo/search?q={SearchTerm} iniciada", q);

                var documentosModelo = await _service.SearchDocumentoModelosAsync(q);

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/DocumentoModelo/search?q={SearchTerm} completado en {ElapsedMs}ms. Retornando {Count} documentos modelo",
                    q, stopwatch.ElapsedMilliseconds, documentosModelo.Count());

                return Ok(documentosModelo);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/DocumentoModelo/search?q={SearchTerm} después de {ElapsedMs}ms", q, stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "No se pudo realizar la búsqueda de documentos modelo",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
