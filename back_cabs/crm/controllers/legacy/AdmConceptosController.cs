// =====================================================================================
// CONTROLADOR ADM CONCEPTOS - AdmConceptosController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// API REST para conceptos legacy de admConceptos (adCABS2016).
// Expone endpoints paginados para consulta y búsqueda sin cache Redis.
//
// ENDPOINTS:
// - GET /api/AdmConceptos/paginated?page={page}&pageSize={pageSize}
// - GET /api/AdmConceptos/search/paginated?q={term}&page={page}&pageSize={pageSize}
//
// CARACTERÍSTICAS:
// - Autorización: Roles ADMINISTRACION, RECEPCION
// - Paginación: 50 registros por defecto, máximo 100
// - Sin cache Redis: Catálogo pequeño (~89 registros)
// - Búsqueda: Por código y nombre de concepto
// - Logging detallado con métricas de rendimiento
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Interfaces.Legacy;
using CRM.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace back_cabs.CRM.controllers
{
    /// <summary>
    /// Controlador REST para conceptos legacy de admConceptos (adCABS2016)
    /// Proporciona acceso paginado a catálogo de conceptos del sistema Adminpaq
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AdmConceptosController : ControllerBase
    {
        private readonly IAdmConceptoService _service;
        private readonly ILogger<AdmConceptosController> _logger;

        public AdmConceptosController(
            IAdmConceptoService service,
            ILogger<AdmConceptosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────
        // 📄 GET PAGINATED - Obtener conceptos legacy paginados
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todos los conceptos legacy con paginación
        /// </summary>
        /// <param name="page">Número de página (1-based). Default: 1</param>
        /// <param name="pageSize">Registros por página (1-100). Default: 50</param>
        /// <response code="200">Conceptos obtenidos exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(PaginatedResponseDto<AdmConceptoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📄 Solicitud GET /api/AdmConceptos/paginated?page={Page}&pageSize={PageSize} iniciada",
                    page, pageSize);

                var paginatedResult = await _service.GetAllPaginatedAsync(page, pageSize);

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/AdmConceptos/paginated completado en {ElapsedMs}ms. Retornando página {Page} de {TotalPages} ({Count} de {TotalItems} conceptos legacy)",
                    stopwatch.ElapsedMilliseconds, paginatedResult.Pagina, paginatedResult.TotalPaginas,
                    paginatedResult.Items.Count(), paginatedResult.TotalItems);

                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/AdmConceptos/paginated después de {ElapsedMs}ms",
                    stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "Ocurrió un error al obtener los conceptos legacy",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 🔍 SEARCH PAGINATED - Búsqueda paginada de conceptos legacy
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Busca conceptos legacy por código o nombre con paginación
        /// </summary>
        /// <param name="q">Término de búsqueda (código o nombre del concepto)</param>
        /// <param name="page">Número de página (1-based). Default: 1</param>
        /// <param name="pageSize">Registros por página (1-100). Default: 50</param>
        /// <response code="200">Búsqueda exitosa</response>
        /// <response code="400">Término de búsqueda inválido</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("search/paginated")]
        [ProducesResponseType(typeof(PaginatedResponseDto<AdmConceptoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchPaginated(
            [FromQuery] string q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
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

                _logger.LogInformation("🔍 Solicitud GET /api/AdmConceptos/search/paginated?q={SearchTerm}&page={Page}&pageSize={PageSize} iniciada",
                    q, page, pageSize);

                var paginatedResult = await _service.SearchPaginatedAsync(q, page, pageSize);

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/AdmConceptos/search/paginated completado en {ElapsedMs}ms. Búsqueda '{SearchTerm}': página {Page} de {TotalPages} ({Count} de {TotalItems} conceptos legacy)",
                    stopwatch.ElapsedMilliseconds, q, paginatedResult.Pagina, paginatedResult.TotalPaginas,
                    paginatedResult.Items.Count(), paginatedResult.TotalItems);

                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/AdmConceptos/search/paginated ('{SearchTerm}') después de {ElapsedMs}ms",
                    q, stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "Ocurrió un error al buscar conceptos legacy",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}