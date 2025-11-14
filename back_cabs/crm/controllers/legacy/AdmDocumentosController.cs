using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace back_cabs.CRM.controllers.Legacy
{
    /// <summary>
    /// Controlador para AdmDocumentos (Cotizaciones/Documentos) - Legacy Adminpaq
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AdmDocumentosController : ControllerBase
    {
        private readonly IAdmDocumentoService _service;
        private readonly ILogger<AdmDocumentosController> _logger;

        public AdmDocumentosController(
            IAdmDocumentoService service,
            ILogger<AdmDocumentosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Busca documentos (cotizaciones) aplicando filtros con paginación
        /// </summary>
        /// <remarks>
        /// Filtros disponibles:
        /// - fechaInicio/fechaFin: Rango de fechas del documento
        /// - folio: Folio del documento (búsqueda exacta)
        /// - razonSocial: Razón social del cliente (búsqueda parcial)
        /// - fechaVencimientoInicio/fechaVencimientoFin: Rango de fechas de vencimiento
        /// - idConcepto: ID del concepto del documento
        /// - idAgente: ID del agente de ventas
        /// - page: Número de página (default: 1)
        /// - pageSize: Registros por página (default: 50, max: 100)
        /// - incluirMovimientos: Incluye los productos cotizados (default: false)
        /// </remarks>
        [HttpGet("search")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] AdmDocumentoFilterDto filter)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                // Validación básica de fechas
                if (filter.FechaInicio.HasValue && filter.FechaFin.HasValue &&
                    filter.FechaInicio.Value > filter.FechaFin.Value)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La fecha inicial no puede ser mayor que la fecha final"
                    });
                }

                if (filter.FechaVencimientoInicio.HasValue && filter.FechaVencimientoFin.HasValue &&
                    filter.FechaVencimientoInicio.Value > filter.FechaVencimientoFin.Value)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La fecha de vencimiento inicial no puede ser mayor que la final"
                    });
                }

                var (documentos, totalRegistros) = await _service.SearchPaginatedAsync(filter);

                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)filter.PageSize);

                sw.Stop();
                _logger.LogInformation(
                    "✅ GET /api/AdmDocumentos/search completado en {Ms}ms. Página {Page}/{TotalPages}, Total: {Total}",
                    sw.ElapsedMilliseconds, filter.Page, totalPaginas, totalRegistros);

                return Ok(new
                {
                    success = true,
                    data = documentos,
                    pagination = new
                    {
                        currentPage = filter.Page,
                        pageSize = filter.PageSize,
                        totalPages = totalPaginas,
                        totalRecords = totalRegistros,
                        hasNextPage = filter.Page < totalPaginas,
                        hasPreviousPage = filter.Page > 1
                    },
                    filters = new
                    {
                        fechaInicio = filter.FechaInicio,
                        fechaFin = filter.FechaFin,
                        folio = filter.Folio,
                        razonSocial = filter.RazonSocial,
                        fechaVencimientoInicio = filter.FechaVencimientoInicio,
                        fechaVencimientoFin = filter.FechaVencimientoFin,
                        idConcepto = filter.IdConcepto,
                        idAgente = filter.IdAgente,
                        incluirMovimientos = filter.IncluirMovimientos
                    },
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "❌ Error en búsqueda de documentos después de {Ms}ms",
                    sw.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al buscar documentos",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene un documento específico por ID incluyendo sus movimientos (productos cotizados)
        /// </summary>
        /// <param name="id">ID del documento</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var documento = await _service.GetByIdWithMovimientosAsync(id);

                if (documento == null)
                {
                    sw.Stop();
                    _logger.LogWarning(
                        "⚠️ Documento {Id} no encontrado después de {Ms}ms",
                        id, sw.ElapsedMilliseconds);

                    return NotFound(new
                    {
                        success = false,
                        message = $"Documento {id} no encontrado"
                    });
                }

                sw.Stop();
                _logger.LogInformation(
                    "✅ GET /api/AdmDocumentos/{Id} completado en {Ms}ms. Movimientos: {MovCount}",
                    id, sw.ElapsedMilliseconds, documento.Movimientos.Count);

                return Ok(new
                {
                    success = true,
                    data = documento,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "❌ Error al obtener documento {Id} después de {Ms}ms",
                    id, sw.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener documento {id}",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene un resumen rápido de documentos por fecha
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial</param>
        /// <param name="fechaFin">Fecha final</param>
        [HttpGet("resumen")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetResumen(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (!fechaInicio.HasValue || !fechaFin.HasValue)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Se requieren fechaInicio y fechaFin"
                    });
                }

                if (fechaInicio.Value > fechaFin.Value)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La fecha inicial no puede ser mayor que la fecha final"
                    });
                }

                var filter = new AdmDocumentoFilterDto
                {
                    FechaInicio = fechaInicio.Value,
                    FechaFin = fechaFin.Value,
                    Page = 1,
                    PageSize = 1, // Solo necesitamos el total
                    IncluirMovimientos = false
                };

                var (_, totalRegistros) = await _service.SearchPaginatedAsync(filter);

                sw.Stop();
                _logger.LogInformation(
                    "✅ GET /api/AdmDocumentos/resumen completado en {Ms}ms. Total: {Total}",
                    sw.ElapsedMilliseconds, totalRegistros);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        fechaInicio = fechaInicio.Value,
                        fechaFin = fechaFin.Value,
                        totalDocumentos = totalRegistros
                    },
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "❌ Error al obtener resumen después de {Ms}ms",
                    sw.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener resumen",
                    error = ex.Message
                });
            }
        }
    }
}
