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

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS POST (CREACIÓN)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea un nuevo documento en el sistema legacy
        /// </summary>
        /// <remarks>
        /// Campos requeridos:
        /// - IdDocumentoDe: ID del modelo de documento (FK a admDocumentosModelo)
        /// - IdConceptoDocumento: ID del concepto (FK a admConceptos)
        /// - IdClienteProveedor: ID del cliente/proveedor (FK a admClientes)
        /// - IdAgente: ID del agente (FK a admAgentes)
        /// - IdMoneda: ID de la moneda (FK a admMonedas)
        /// - SerieDocumento: Serie del documento (máx 11 caracteres)
        /// - Folio: Número de folio
        /// - FechaVencimiento: Fecha de vencimiento
        /// - TipoCambio: Tipo de cambio de la moneda
        /// - Referencia: Referencia del documento (máx 20 caracteres)
        /// - Naturaleza: 1=Cargo, 2=Abono
        /// - UsaCliente: 1=Cliente, 0=Proveedor
        /// - Afectado: 1=Sí, 0=No
        /// - Impreso: 1=Sí, 0=No
        /// - Cancelado: 1=Sí, 0=No
        /// - Neto: Subtotal antes de impuestos
        /// - Impuesto1: Impuesto aplicado (IVA)
        /// - DescuentoMov: Descuento a nivel movimiento
        /// - Total: Total del documento
        /// - Pendiente: Saldo pendiente
        /// - TotalUnidades: Cantidad total de productos/servicios
        /// 
        /// Campos opcionales:
        /// - Observaciones: Notas del documento
        /// 
        /// El sistema genera automáticamente:
        /// - CFecha (fecha actual)
        /// - CGuidDocumento (GUID único)
        /// - CTimestamp (timestamp del sistema)
        /// - CRazonSocial y CRfc (obtenidos del cliente/proveedor)
        /// </remarks>
        /// <param name="dto">Datos del documento a crear</param>
        /// <response code="201">Documento creado exitosamente</response>
        /// <response code="400">Datos inválidos o validación fallida</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AdmDocumentoCreateDto dto)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("📝 POST /api/AdmDocumentos - Iniciando creación. Serie: {Serie}, Folio: {Folio}", 
                    dto.SerieDocumento, dto.Folio);

                // Validar ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("⚠️ Validación fallida. Errores: {Errores}", string.Join(", ", errors));

                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos inválidos",
                        errors = errors
                    });
                }

                // Validaciones adicionales de negocio
                if (dto.Total <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El total del documento debe ser mayor a 0"
                    });
                }

                if (dto.Pendiente < 0 || dto.Pendiente > dto.Total)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El pendiente debe estar entre 0 y el total del documento"
                    });
                }

                if (dto.FechaVencimiento < DateTime.Today)
                {
                    _logger.LogWarning("⚠️ Fecha de vencimiento {FechaVencimiento} es menor a la fecha actual", 
                        dto.FechaVencimiento);
                }

                // Crear documento
                var idDocumento = await _service.CreateAsync(dto);

                sw.Stop();
                _logger.LogInformation(
                    "✅ POST /api/AdmDocumentos completado en {Ms}ms. ID creado: {IdDocumento}, Serie: {Serie}, Folio: {Folio}",
                    sw.ElapsedMilliseconds, idDocumento, dto.SerieDocumento, dto.Folio);

                // Retornar 201 Created con Location header
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = idDocumento },
                    new
                    {
                        success = true,
                        message = "Documento creado exitosamente",
                        data = new
                        {
                            idDocumento = idDocumento,
                            serieDocumento = dto.SerieDocumento,
                            folio = dto.Folio,
                            total = dto.Total,
                            fechaCreacion = DateTime.Now
                        },
                        executionTime = $"{sw.ElapsedMilliseconds}ms"
                    });
            }
            catch (InvalidOperationException ex)
            {
                // Errores de validación de relaciones (FKs no existen)
                sw.Stop();
                _logger.LogWarning(ex,
                    "⚠️ Validación fallida en creación de documento después de {Ms}ms. Serie: {Serie}, Folio: {Folio}",
                    sw.ElapsedMilliseconds, dto.SerieDocumento, dto.Folio);

                return BadRequest(new
                {
                    success = false,
                    message = "Validación fallida",
                    error = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "❌ Error al crear documento después de {Ms}ms. Serie: {Serie}, Folio: {Folio}",
                    sw.ElapsedMilliseconds, dto.SerieDocumento, dto.Folio);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear documento",
                    error = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // ENDPOINT MEJORADO PARA COTIZACIONES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea una nueva cotización de forma simplificada (ENDPOINT MEJORADO)
        /// </summary>
        /// <remarks>
        /// Este endpoint facilita la creación de cotizaciones con valores por defecto automáticos.
        /// 
        /// **Campos que el usuario debe proporcionar:**
        /// - `idCliente` (obligatorio): ID del cliente obtenido mediante búsqueda previa
        /// - `productos` (obligatorio): Array con los productos a cotizar
        ///   - `idProducto`: ID del producto
        ///   - `idAlmacen`: ID del almacén
        ///   - `unidades`: Cantidad
        ///   - `precio`: Precio unitario
        ///   - `porcentajeDescuento` (opcional): Descuento individual del producto
        /// 
        /// **Campos opcionales:**
        /// - `idAgente`: Agente de ventas (default: 1)
        /// - `fechaVencimiento`: Fecha de vencimiento (default: 30 días después)
        /// - `descuentoDoc1`: Descuento global 1 (porcentaje)
        /// - `descuentoDoc2`: Descuento global 2 (porcentaje)
        /// - `montoPagado`: Monto que el cliente pagó o abonó
        /// - `aplicarIVA`: Si se aplica IVA (default: true)
        /// - `porcentajeIVA`: Porcentaje de IVA (default: 16%)
        /// - `observaciones`: Notas adicionales
        /// - `referencia`: Referencia del documento
        /// 
        /// **Campos automáticos (el sistema los genera):**
        /// - Serie: "CA"
        /// - Folio: Auto-incremental (se obtiene de admConceptos)
        /// - Fecha: Fecha actual
        /// - IdDocumentoDe: 1
        /// - IdConceptoDocumento: 1
        /// - TipoCambio: 1
        /// - IdMoneda: 1 (MXN)
        /// - Naturaleza: 1 (Cargo)
        /// - SistOrig: 205
        /// - Afectado: 1
        /// - Impreso: 0
        /// - Cancelado: 0
        /// - Devuelto: 0
        /// - IdPrepoliza: 0
        /// - Destinatario: Igual a razón social del cliente
        /// - Total, Neto, Impuestos: Calculados automáticamente
        /// - Pendiente: Total - MontoPagado
        /// 
        /// **Ejemplo de petición:**
        /// ```json
        /// {
        ///   "idCliente": 123,
        ///   "idAgente": 1,
        ///   "productos": [
        ///     {
        ///       "idProducto": 10,
        ///       "idAlmacen": 1,
        ///       "unidades": 5,
        ///       "precio": 100.00,
        ///       "porcentajeDescuento": 10
        ///     }
        ///   ],
        ///   "descuentoDoc1": 5,
        ///   "montoPagado": 200,
        ///   "aplicarIVA": true,
        ///   "observaciones": "Cotización urgente"
        /// }
        /// ```
        /// </remarks>
        /// <param name="dto">DTO simplificado con los datos de la cotización</param>
        /// <returns>Datos de la cotización creada con folio asignado</returns>
        [HttpPost("cotizacion")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCotizacion([FromBody] AdmCotizacionCreateDto dto)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("🚀 [POST] /api/AdmDocumentos/cotizacion - Cliente: {IdCliente}, Productos: {CantidadProductos}",
                    dto.IdCliente, dto.Productos?.Count ?? 0);

                // Validar ModelState
                if (!ModelState.IsValid)
                {
                    sw.Stop();
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("⚠️ Validación fallida después de {Ms}ms. Errores: {Errores}",
                        sw.ElapsedMilliseconds, string.Join(", ", errors));

                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos de entrada inválidos",
                        errors = errors,
                        executionTime = $"{sw.ElapsedMilliseconds}ms"
                    });
                }

                // Crear cotización
                var response = await _service.CreateCotizacionAsync(dto);

                sw.Stop();
                _logger.LogInformation("✅ Cotización creada exitosamente después de {Ms}ms. ID: {IdDocumento}, Folio: {Folio}",
                    sw.ElapsedMilliseconds, response.IdDocumento, response.Folio);

                return StatusCode(201, new
                {
                    success = true,
                    message = response.Mensaje,
                    data = response,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (InvalidOperationException ex)
            {
                sw.Stop();
                _logger.LogWarning(ex, "⚠️ Operación inválida después de {Ms}ms. Cliente: {IdCliente}",
                    sw.ElapsedMilliseconds, dto.IdCliente);

                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al crear cotización después de {Ms}ms. Cliente: {IdCliente}",
                    sw.ElapsedMilliseconds, dto.IdCliente);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear cotización",
                    error = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
        }

        /// <summary>
        /// Cancela una cotización existente (ENDPOINT PUT)
        /// </summary>
        /// <remarks>
        /// Este endpoint permite cancelar una cotización existente cambiando el campo CCANCELADO a 1.
        /// 
        /// **Campos requeridos:**
        /// - `idDocumento` (obligatorio): ID del documento a cancelar
        /// 
        /// **Campos opcionales:**
        /// - `motivo`: Motivo de la cancelación
        /// - `usuarioCancela`: Usuario que cancela (si no se envía, se usa "SISTEMA")
        /// 
        /// **Validaciones:**
        /// - El documento debe existir
        /// - El documento no debe estar previamente cancelado
        /// 
        /// **Ejemplo de petición:**
        /// ```json
        /// {
        ///   "idDocumento": 12345,
        ///   "motivo": "Cliente solicitó cancelación",
        ///   "usuarioCancela": "ADMIN"
        /// }
        /// ```
        /// </remarks>
        /// <param name="dto">DTO con los datos de cancelación</param>
        /// <returns>Datos de la cotización cancelada</returns>
        [HttpPut("cotizacion/cancelar")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelarCotizacion([FromBody] AdmCotizacionCancelarDto dto)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("🚫 [PUT] /api/AdmDocumentos/cotizacion/cancelar - Documento: {IdDocumento}",
                    dto.IdDocumento);

                // Validar ModelState
                if (!ModelState.IsValid)
                {
                    sw.Stop();
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("⚠️ Validación fallida después de {Ms}ms. Errores: {Errores}",
                        sw.ElapsedMilliseconds, string.Join(", ", errors));

                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos de entrada inválidos",
                        errors = errors,
                        executionTime = $"{sw.ElapsedMilliseconds}ms"
                    });
                }

                // Cancelar cotización
                var response = await _service.CancelarCotizacionAsync(dto);

                sw.Stop();
                _logger.LogInformation("✅ Cotización cancelada exitosamente después de {Ms}ms. ID: {IdDocumento}, Folio: {Folio}",
                    sw.ElapsedMilliseconds, response.IdDocumento, response.Folio);

                return Ok(new
                {
                    success = true,
                    message = response.Mensaje,
                    data = response,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (InvalidOperationException ex)
            {
                sw.Stop();
                _logger.LogWarning(ex, "⚠️ Operación inválida después de {Ms}ms. Documento: {IdDocumento}",
                    sw.ElapsedMilliseconds, dto.IdDocumento);

                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al cancelar cotización después de {Ms}ms. Documento: {IdDocumento}",
                    sw.ElapsedMilliseconds, dto.IdDocumento);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al cancelar cotización",
                    error = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
        }
    }
}
