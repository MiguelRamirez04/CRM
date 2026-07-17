// =====================================================================================
// CONTROLADOR ADM AGENTES - AdmAgentesController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// API REST para agentes legacy de admAgentes (adCABS2016).
// Expone endpoints paginados para consulta y búsqueda con cache Redis.
//
// ENDPOINTS:
// - GET /api/AdmAgentes/paginated?page={page}&pageSize={pageSize}
// - GET /api/AdmAgentes/search/paginated?q={term}&page={page}&pageSize={pageSize}
//
// CARACTERÍSTICAS:
// - Autorización: Roles ADMINISTRACION, RECEPCION
// - Paginación: 30 registros por defecto, máximo 100
// - Cache Redis: 30min (GetAll), 60min (Search)
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
    /// Controlador REST para agentes legacy de admAgentes (adCABS2016)
    /// Proporciona acceso paginado a catálogo de agentes del sistema Adminpaq
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AdmAgentesController : ControllerBase
    {
        private readonly IAdmAgenteService _service;
        private readonly ILogger<AdmAgentesController> _logger;

        public AdmAgentesController(
            IAdmAgenteService service,
            ILogger<AdmAgentesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────
        // 📄 GET PAGINATED - Obtener agentes legacy paginados
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todos los agentes legacy con paginación
        /// </summary>
        /// <param name="page">Número de página (1-based). Default: 1</param>
        /// <param name="pageSize">Registros por página (1-100). Default: 30</param>
        /// <response code="200">Agentes obtenidos exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(PaginatedResponseDto<AdmAgenteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 30)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📄 Solicitud GET /api/AdmAgentes/paginated?page={Page}&pageSize={PageSize} iniciada",
                    page, pageSize);

                var paginatedResult = await _service.GetAllPaginatedAsync(page, pageSize);

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/AdmAgentes/paginated completado en {ElapsedMs}ms. Retornando página {Page} de {TotalPages} ({Count} de {TotalItems} agentes legacy)",
                    stopwatch.ElapsedMilliseconds, paginatedResult.Pagina, paginatedResult.TotalPaginas,
                    paginatedResult.Items.Count(), paginatedResult.TotalItems);

                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/AdmAgentes/paginated después de {ElapsedMs}ms",
                    stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "Ocurrió un error al obtener los agentes legacy",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 🔍 SEARCH PAGINATED - Búsqueda paginada de agentes legacy
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Busca agentes legacy por código o nombre con paginación
        /// </summary>
        /// <param name="q">Término de búsqueda (código o nombre del agente)</param>
        /// <param name="page">Número de página (1-based). Default: 1</param>
        /// <param name="pageSize">Registros por página (1-100). Default: 30</param>
        /// <response code="200">Búsqueda exitosa</response>
        /// <response code="400">Término de búsqueda inválido</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("search/paginated")]
        [ProducesResponseType(typeof(PaginatedResponseDto<AdmAgenteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchPaginated(
            [FromQuery] string q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 30)
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

                _logger.LogInformation("🔍 Solicitud GET /api/AdmAgentes/search/paginated?q={SearchTerm}&page={Page}&pageSize={PageSize} iniciada",
                    q, page, pageSize);

                var paginatedResult = await _service.SearchPaginatedAsync(q, page, pageSize);

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/AdmAgentes/search/paginated completado en {ElapsedMs}ms. Búsqueda '{SearchTerm}': página {Page} de {TotalPages} ({Count} de {TotalItems} agentes legacy)",
                    stopwatch.ElapsedMilliseconds, q, paginatedResult.Pagina, paginatedResult.TotalPaginas,
                    paginatedResult.Items.Count(), paginatedResult.TotalItems);

                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/AdmAgentes/search/paginated ('{SearchTerm}') después de {ElapsedMs}ms",
                    q, stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "Ocurrió un error al buscar agentes legacy",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
