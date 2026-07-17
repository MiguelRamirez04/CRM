// =====================================================================================
// CONTROLADOR ADM DOCUMENTOS MODELO - AdmDocumentosModeloController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// API REST para modelos de documentos legacy de admDocumentosModelo (adCABS2016).
// Expone endpoints simples para consulta completa y búsqueda sin paginación.
//
// ENDPOINTS:
// - GET /api/AdmDocumentosModelo
// - GET /api/AdmDocumentosModelo/search?q={term}
//
// CARACTERÍSTICAS:
// - Autorización: Roles ADMINISTRACION, RECEPCION
// - Sin paginación: Catálogo pequeño (~38 registros)
// - Sin cache Redis: Consulta directa a BD
// - Búsqueda: Por descripción
// - Logging detallado con métricas de rendimiento
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
    /// Controlador REST para modelos de documentos legacy de admDocumentosModelo (adCABS2016)
    /// Proporciona acceso completo a catálogo de modelos de documentos del sistema Adminpaq
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AdmDocumentosModeloController : ControllerBase
    {
        private readonly IAdmDocumentoModeloService _service;
        private readonly ILogger<AdmDocumentosModeloController> _logger;

        public AdmDocumentosModeloController(
            IAdmDocumentoModeloService service,
            ILogger<AdmDocumentosModeloController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────
        // 📄 GET ALL - Obtener todos los modelos de documentos legacy
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todos los modelos de documentos legacy sin paginación
        /// </summary>
        /// <response code="200">Modelos de documentos obtenidos exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<AdmDocumentoModeloResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📄 Solicitud GET /api/AdmDocumentosModelo iniciada");

                var documentosModelo = await _service.GetAllAsync();

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/AdmDocumentosModelo completado en {ElapsedMs}ms. Retornando {Count} modelos de documentos legacy",
                    stopwatch.ElapsedMilliseconds, documentosModelo.Count);

                return Ok(documentosModelo);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/AdmDocumentosModelo después de {ElapsedMs}ms",
                    stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "Ocurrió un error al obtener los modelos de documentos legacy",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 🔍 SEARCH - Búsqueda de modelos de documentos por descripción
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Busca modelos de documentos legacy por descripción
        /// </summary>
        /// <param name="q">Término de búsqueda (descripción del modelo)</param>
        /// <response code="200">Búsqueda exitosa</response>
        /// <response code="400">Término de búsqueda inválido</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<AdmDocumentoModeloResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Validación de parámetros
                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                {
                    _logger.LogWarning("⚠️ Término de búsqueda inválido: '{SearchTerm}'", q);
                    return BadRequest(new
                    {
                        error = "Término de búsqueda inválido",
                        message = "El término de búsqueda debe tener al menos 2 caracteres",
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("🔍 Solicitud GET /api/AdmDocumentosModelo/search?q={SearchTerm} iniciada", q);

                var documentosModelo = await _service.SearchByDescripcionAsync(q);

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/AdmDocumentosModelo/search completado en {ElapsedMs}ms. Búsqueda '{SearchTerm}': {Count} modelos de documentos legacy encontrados",
                    stopwatch.ElapsedMilliseconds, q, documentosModelo.Count);

                return Ok(documentosModelo);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/AdmDocumentosModelo/search ('{SearchTerm}') después de {ElapsedMs}ms",
                    q, stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "Ocurrió un error al buscar modelos de documentos legacy",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}