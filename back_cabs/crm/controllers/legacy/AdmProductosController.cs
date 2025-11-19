// =====================================================================================
// CONTROLADOR ADM PRODUCTOS - AdmProductosController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// API REST para productos legacy de admProductos (adCABS2016).
// Expone endpoints paginados para consulta y búsqueda con cache Redis robusto.
//
// ENDPOINTS:
// - GET /api/AdmProductos/paginated?page={page}&pageSize={pageSize}
// - GET /api/AdmProductos/search/paginated?q={term}&page={page}&pageSize={pageSize}
//
// CARACTERÍSTICAS:
// - Autorización: Roles ADMINISTRACION, RECEPCION
// - Paginación: 50 registros por defecto, máximo 100
// - Cache Redis: 30min (GetAll), 60min (Search)
// - Búsqueda: Por código y nombre de producto
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
    /// Controlador REST para productos legacy de admProductos (adCABS2016)
    /// Proporciona acceso paginado a catálogo de productos del sistema Adminpaq
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AdmProductosController : ControllerBase
    {
        private readonly IAdmProductoService _service;
        private readonly ILogger<AdmProductosController> _logger;

        public AdmProductosController(
            IAdmProductoService service,
            ILogger<AdmProductosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────
        // 📄 GET PAGINATED - Obtener productos legacy paginados
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todos los productos legacy con paginación
        /// </summary>
        /// <param name="page">Número de página (1-based). Default: 1</param>
        /// <param name="pageSize">Registros por página (1-100). Default: 50</param>
        /// <response code="200">Productos obtenidos exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(PaginatedResponseDto<AdmProductoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📄 Solicitud GET /api/AdmProductos/paginated?page={Page}&pageSize={PageSize} iniciada",
                    page, pageSize);

                var paginatedResult = await _service.GetAllPaginatedAsync(page, pageSize);

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/AdmProductos/paginated completado en {ElapsedMs}ms. Retornando página {Page} de {TotalPages} ({Count} de {TotalItems} productos legacy)",
                    stopwatch.ElapsedMilliseconds, paginatedResult.Pagina, paginatedResult.TotalPaginas,
                    paginatedResult.Items.Count(), paginatedResult.TotalItems);

                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/AdmProductos/paginated después de {ElapsedMs}ms",
                    stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "Ocurrió un error al obtener los productos legacy",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 🔍 SEARCH PAGINATED - Búsqueda paginada de productos legacy
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Busca productos legacy por código o nombre con paginación
        /// </summary>
        /// <param name="q">Término de búsqueda (código o nombre del producto)</param>
        /// <param name="page">Número de página (1-based). Default: 1</param>
        /// <param name="pageSize">Registros por página (1-100). Default: 50</param>
        /// <response code="200">Búsqueda exitosa</response>
        /// <response code="400">Término de búsqueda inválido</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("search/paginated")]
        [ProducesResponseType(typeof(PaginatedResponseDto<AdmProductoResponseDto>), StatusCodes.Status200OK)]
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

                _logger.LogInformation("🔍 Solicitud GET /api/AdmProductos/search/paginated?q={SearchTerm}&page={Page}&pageSize={PageSize} iniciada",
                    q, page, pageSize);

                var paginatedResult = await _service.SearchPaginatedAsync(q, page, pageSize);

                stopwatch.Stop();
                _logger.LogInformation("✅ GET /api/AdmProductos/search/paginated completado en {ElapsedMs}ms. Búsqueda '{SearchTerm}': página {Page} de {TotalPages} ({Count} de {TotalItems} productos legacy)",
                    stopwatch.ElapsedMilliseconds, q, paginatedResult.Pagina, paginatedResult.TotalPaginas,
                    paginatedResult.Items.Count(), paginatedResult.TotalItems);

                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error en GET /api/AdmProductos/search/paginated ('{SearchTerm}') después de {ElapsedMs}ms",
                    q, stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = "Ocurrió un error al buscar productos legacy",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // ENDPOINT AUXILIAR PARA COTIZACIONES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Busca productos por código o nombre (para uso en cotizaciones)
        /// </summary>
        /// <remarks>
        /// Endpoint simplificado para búsqueda rápida de productos al crear cotizaciones.
        /// Busca coincidencias parciales en código o nombre de producto.
        /// 
        /// **Ejemplos de uso:**
        /// - GET /api/AdmProductos/buscar?texto=LAPTOP
        /// - GET /api/AdmProductos/buscar?texto=MON-001
        /// 
        /// **Respuesta:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "data": [
        ///     {
        ///       "idProducto": 456,
        ///       "codigoProducto": "LAP-001",
        ///       "nombreProducto": "LAPTOP HP PROBOOK 450",
        ///       "descripcion": "LAPTOP HP PROBOOK 450 G10",
        ///       "precio": 15000.00,
        ///       "idUnidadBase": 1,
        ///       "nombreUnidad": "PZA"
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="texto">Texto a buscar (código o nombre de producto)</param>
        /// <param name="limit">Límite de resultados (default: 20, max: 50)</param>
        /// <returns>Lista de productos que coinciden con la búsqueda</returns>
        [HttpGet("buscar")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BuscarParaCotizacion(
            [FromQuery] string texto,
            [FromQuery] int limit = 20)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (string.IsNullOrWhiteSpace(texto) || texto.Length < 2)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El parámetro 'texto' debe tener al menos 2 caracteres"
                    });
                }

                if (limit > 50)
                {
                    limit = 50;
                }

                _logger.LogInformation("🔍 Buscando productos con texto: {Texto}, Límite: {Limit}", texto, limit);

                // Usar el método de búsqueda existente
                var paginatedResult = await _service.SearchPaginatedAsync(texto, 1, limit);

                sw.Stop();
                _logger.LogInformation("✅ Búsqueda para cotización completada en {Ms}ms. Resultados: {Count}",
                    sw.ElapsedMilliseconds, paginatedResult.Items.Count());

                // Simplificar respuesta para cotizaciones
                var resultadosSimplificados = paginatedResult.Items.Select(p => new
                {
                    idProducto = p.IdProducto,
                    codigoProducto = p.CodigoProducto,
                    nombreProducto = p.NombreProducto,
                    descripcion = p.Descripcion,
                    precio = p.Precio1, // Precio lista 1
                    idUnidadBase = p.IdUnidadBase
                }).ToList();

                return Ok(new
                {
                    success = true,
                    data = resultadosSimplificados,
                    totalEncontrados = resultadosSimplificados.Count,
                    textoBuscado = texto,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al buscar productos después de {Ms}ms. Texto: {Texto}",
                    sw.ElapsedMilliseconds, texto);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al buscar productos",
                    error = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
        }
    }
}