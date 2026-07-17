// =====================================================================================
// CONTROLADOR ADM MONEDAS - AdmMonedasController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// API REST para monedas legacy de admMonedas (adCABS2016).
// Expone endpoints paginados para consulta con cache Redis.
//
// ENDPOINTS:
// - GET /api/AdmMonedas/paginated?page={page}&pageSize={pageSize}
//
// CARACTERÍSTICAS:
// - Autorización: Roles ADMINISTRACION, RECEPCION
// - Paginación: 30 registros por defecto, máximo 100
// - Cache Redis: 60min (catálogo estable)
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
    /// Controlador REST para monedas legacy de admMonedas (adCABS2016)
    /// Proporciona acceso paginado a catálogo de monedas del sistema Adminpaq
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AdmMonedasController : ControllerBase
    {
        private readonly IAdmMonedaService _service;
        private readonly ILogger<AdmMonedasController> _logger;

        public AdmMonedasController(
            IAdmMonedaService service,
            ILogger<AdmMonedasController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────
        // 📄 GET PAGINATED - Obtener monedas legacy paginadas
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todas las monedas legacy con paginación
        /// </summary>
        /// <param name="page">Número de página (1-based). Default: 1</param>
        /// <param name="pageSize">Registros por página (1-100). Default: 30</param>
        /// <response code="200">Monedas obtenidas exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(PaginatedResponseDto<AdmMonedaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 30)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📄 Solicitud GET /api/AdmMonedas/paginated?page={Page}&pageSize={PageSize} iniciada",
                    page, pageSize);

                var paginatedResult = await _service.GetAllPaginatedAsync(page, pageSize);

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/AdmMonedas/paginated completado en {ElapsedMs}ms. Retornando página {Page} de {TotalPages} ({Count} de {TotalItems} monedas legacy)",
                    stopwatch.ElapsedMilliseconds, paginatedResult.Pagina, paginatedResult.TotalPaginas,
                    paginatedResult.Items.Count(), paginatedResult.TotalItems);

                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/AdmMonedas/paginated después de {ElapsedMs}ms",
                    stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "Ocurrió un error al obtener las monedas legacy",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}