// =====================================================================================
// CONTROLADOR ADM ALMACENES - AdmAlmacenesController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// API REST para almacenes legacy de admAlmacenes (adCABS2016).
// Expone endpoint simple para consulta completa sin paginación.
//
// ENDPOINTS:
// - GET /api/AdmAlmacenes
//
// CARACTERÍSTICAS:
// - Autorización: Roles ADMINISTRACION, RECEPCION
// - Sin paginación: Catálogo pequeño (~5 registros)
// - Sin cache Redis: Consulta directa a BD
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
    /// Controlador REST para almacenes legacy de admAlmacenes (adCABS2016)
    /// Proporciona acceso completo a catálogo de almacenes del sistema Adminpaq
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AdmAlmacenesController : ControllerBase
    {
        private readonly IAdmAlmacenService _service;
        private readonly ILogger<AdmAlmacenesController> _logger;

        public AdmAlmacenesController(
            IAdmAlmacenService service,
            ILogger<AdmAlmacenesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────
        // 📄 GET ALL - Obtener todos los almacenes legacy
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todos los almacenes legacy sin paginación
        /// </summary>
        /// <response code="200">Almacenes obtenidos exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<AdmAlmacenResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📄 Solicitud GET /api/AdmAlmacenes iniciada");

                var almacenes = await _service.GetAllAsync();

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/AdmAlmacenes completado en {ElapsedMs}ms. Retornando {Count} almacenes legacy",
                    stopwatch.ElapsedMilliseconds, almacenes.Count);

                return Ok(almacenes);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/AdmAlmacenes después de {ElapsedMs}ms",
                    stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "Ocurrió un error al obtener los almacenes legacy",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}