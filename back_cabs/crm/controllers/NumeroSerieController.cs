// =====================================================================================
// CONTROLADOR NÚMERO SERIE - NumeroSerieController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// API REST para números de serie con integración de cache Redis.
// Expone endpoints GET para consulta y búsqueda optimizadas.
//
// ENDPOINTS:
// - GET /api/NumeroSerie → Todos los números de serie activos (Cache 30min)
// - GET /api/NumeroSerie/search?q={term} → Búsqueda por número (Cache 60min)
//
// CARACTERÍSTICAS:
// - Autorización: Roles ADMINISTRACION, RECEPCION
// - Logging detallado con métricas de rendimiento
// - Validación de parámetros de entrada
// - Manejo robusto de errores
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Interfaces.Legacy;
using CRM.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace back_cabs.CRM.controllers
{
    /// <summary>
    /// Controlador REST para gestión de números de serie
    /// Endpoints optimizados con cache Redis para alto rendimiento
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class NumeroSerieController : ControllerBase
    {
        private readonly INumeroSerieService _service;
        private readonly ILogger<NumeroSerieController> _logger;

        public NumeroSerieController(INumeroSerieService service, ILogger<NumeroSerieController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────
        // 📄 GET PAGINATED - Obtener números de serie paginados
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todos los números de serie activos con paginación
        /// Recomendado para grandes volúmenes de datos (15,000+ registros)
        /// </summary>
        /// <param name="page">Número de página (1-based). Default: 1</param>
        /// <param name="pageSize">Registros por página (1-100). Default: 30</param>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(PaginatedResponseDto<NumeroSerieResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNumeroSeriePaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 30)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📄 Solicitud GET /api/NumeroSerie/paginated?page={Page}&pageSize={PageSize} iniciada", page, pageSize);

                var paginatedResult = await _service.GetNumeroSeriePaginatedAsync(page, pageSize);

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/NumeroSerie/paginated completado en {ElapsedMs}ms. Retornando página {Page} de {TotalPages} ({Count} de {TotalItems} números de serie)",
                    stopwatch.ElapsedMilliseconds, paginatedResult.Pagina, paginatedResult.TotalPaginas, 
                    paginatedResult.Items.Count(), paginatedResult.TotalItems);

                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/NumeroSerie/paginated después de {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "Ocurrió un error al obtener los números de serie paginados",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 🔍 SEARCH PAGINATED - Buscar números de serie con paginación
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Busca números de serie por término con paginación
        /// </summary>
        /// <param name="q">Término de búsqueda (requerido)</param>
        /// <param name="page">Número de página (1-based). Default: 1</param>
        /// <param name="pageSize">Registros por página (1-100). Default: 30</param>
        [HttpGet("search/paginated")]
        [ProducesResponseType(typeof(PaginatedResponseDto<NumeroSerieResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchNumeroSeriePaginated([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    _logger.LogWarning("⚠️ Parámetro 'q' vacío en búsqueda paginada de números de serie");
                    return BadRequest(new
                    {
                        error = "Parámetro requerido",
                        message = "El parámetro 'q' es obligatorio para la búsqueda",
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("🔍📄 Solicitud GET /api/NumeroSerie/search/paginated?q={SearchTerm}&page={Page}&pageSize={PageSize} iniciada", 
                    q, page, pageSize);

                var paginatedResult = await _service.SearchNumeroSeriePaginatedAsync(q, page, pageSize);

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/NumeroSerie/search/paginated?q={SearchTerm} completado en {ElapsedMs}ms. Retornando página {Page} de {TotalPages} ({Count} de {TotalItems} números de serie)",
                    q, stopwatch.ElapsedMilliseconds, paginatedResult.Pagina, paginatedResult.TotalPaginas, 
                    paginatedResult.Items.Count(), paginatedResult.TotalItems);

                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/NumeroSerie/search/paginated?q={SearchTerm} después de {ElapsedMs}ms", q, stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "Ocurrió un error al buscar números de serie paginados",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}

