using back_cabs.CRM.Interfaces.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace back_cabs.CRM.controllers.Legacy
{
    /// <summary>
    /// Controlador para gestión de movimientos serie
    /// Relación entre movimientos de inventario y números de serie
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AdmMovimientosSerieController : ControllerBase
    {
        private readonly IAdmMovimientoSerieService _service;
        private readonly ILogger<AdmMovimientosSerieController> _logger;

        public AdmMovimientosSerieController(
            IAdmMovimientoSerieService service,
            ILogger<AdmMovimientosSerieController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los movimientos serie con paginación
        /// </summary>
        /// <param name="page">Número de página (default: 1)</param>
        /// <param name="pageSize">Tamaño de página (default: 50, max: 100)</param>
        /// <returns>Lista paginada de movimientos serie</returns>
        /// <response code="200">Lista obtenida exitosamente</response>
        /// <response code="400">Parámetros de paginación inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                // Validación de parámetros
                if (page < 1)
                {
                    _logger.LogWarning("⚠️ Número de página inválido: {Page}", page);
                    return BadRequest(new { success = false, message = "El número de página debe ser mayor a 0" });
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    _logger.LogWarning("⚠️ Tamaño de página inválido: {PageSize}", pageSize);
                    return BadRequest(new { success = false, message = "El tamaño de página debe estar entre 1 y 100" });
                }

                _logger.LogInformation("📋 Obteniendo movimientos serie paginados. Página: {Page}, Tamaño: {PageSize}", page, pageSize);

                var (items, totalCount) = await _service.GetAllPaginatedAsync(page, pageSize);

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                sw.Stop();
                _logger.LogInformation("✅ Movimientos serie obtenidos en {Elapsed}ms. Total: {Count}", 
                    sw.ElapsedMilliseconds, totalCount);

                return Ok(new
                {
                    success = true,
                    data = items,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize,
                        totalPages,
                        totalRecords = totalCount,
                        hasNextPage = page < totalPages,
                        hasPreviousPage = page > 1
                    },
                    message = $"Se encontraron {totalCount} movimiento(s) serie"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener movimientos serie después de {Elapsed}ms", sw.ElapsedMilliseconds);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener los movimientos serie",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener movimiento serie por ID
        /// </summary>
        /// <param name="id">ID del movimiento serie</param>
        /// <returns>Datos del movimiento serie</returns>
        /// <response code="200">Movimiento serie encontrado</response>
        /// <response code="404">Movimiento serie no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("🔍 Buscando movimiento serie ID: {Id}", id);

                var movimientoSerie = await _service.GetByIdAsync(id);

                if (movimientoSerie == null)
                {
                    sw.Stop();
                    _logger.LogWarning("⚠️ Movimiento serie {Id} no encontrado después de {Elapsed}ms", id, sw.ElapsedMilliseconds);
                    
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontró el movimiento serie con ID {id}"
                    });
                }

                sw.Stop();
                _logger.LogInformation("✅ Movimiento serie {Id} encontrado en {Elapsed}ms", id, sw.ElapsedMilliseconds);

                return Ok(new
                {
                    success = true,
                    data = movimientoSerie,
                    message = "Movimiento serie encontrado"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener movimiento serie {Id} después de {Elapsed}ms", id, sw.ElapsedMilliseconds);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener el movimiento serie",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener movimientos serie por ID de movimiento
        /// </summary>
        /// <param name="idMovimiento">ID del movimiento de inventario</param>
        /// <returns>Lista de números de serie asociados al movimiento</returns>
        /// <response code="200">Lista obtenida exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("by-movimiento/{idMovimiento}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByMovimientoId(int idMovimiento)
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("🔍 Buscando movimientos serie para movimiento ID: {IdMovimiento}", idMovimiento);

                var movimientosSerie = await _service.GetByMovimientoIdAsync(idMovimiento);

                sw.Stop();
                _logger.LogInformation("✅ Búsqueda completada en {Elapsed}ms. Resultados: {Count}", 
                    sw.ElapsedMilliseconds, movimientosSerie.Count);

                return Ok(new
                {
                    success = true,
                    data = movimientosSerie,
                    total = movimientosSerie.Count,
                    idMovimiento,
                    message = movimientosSerie.Any() 
                        ? $"Se encontraron {movimientosSerie.Count} número(s) de serie para el movimiento {idMovimiento}" 
                        : $"No se encontraron números de serie para el movimiento {idMovimiento}"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener movimientos serie por ID movimiento después de {Elapsed}ms", sw.ElapsedMilliseconds);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener los movimientos serie",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener movimientos por ID de número de serie
        /// </summary>
        /// <param name="idSerie">ID del número de serie</param>
        /// <returns>Lista de movimientos donde se ha usado este número de serie</returns>
        /// <response code="200">Lista obtenida exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("by-serie/{idSerie}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBySerieId(int idSerie)
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("🔍 Buscando movimientos para número de serie ID: {IdSerie}", idSerie);

                var movimientosSerie = await _service.GetBySerieIdAsync(idSerie);

                sw.Stop();
                _logger.LogInformation("✅ Búsqueda completada en {Elapsed}ms. Resultados: {Count}", 
                    sw.ElapsedMilliseconds, movimientosSerie.Count);

                return Ok(new
                {
                    success = true,
                    data = movimientosSerie,
                    total = movimientosSerie.Count,
                    idSerie,
                    message = movimientosSerie.Any() 
                        ? $"Se encontraron {movimientosSerie.Count} movimiento(s) para el número de serie {idSerie}" 
                        : $"No se encontraron movimientos para el número de serie {idSerie}"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener movimientos serie por ID serie después de {Elapsed}ms", sw.ElapsedMilliseconds);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener los movimientos serie",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener movimientos serie por rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial (formato: yyyy-MM-dd)</param>
        /// <param name="fechaFin">Fecha final (formato: yyyy-MM-dd)</param>
        /// <returns>Lista de movimientos serie en el rango de fechas</returns>
        /// <response code="200">Lista obtenida exitosamente</response>
        /// <response code="400">Parámetros de fecha inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("by-date-range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                // Validación
                if (fechaFin < fechaInicio)
                {
                    _logger.LogWarning("⚠️ Rango de fechas inválido: {FechaInicio} - {FechaFin}", fechaInicio, fechaFin);
                    return BadRequest(new 
                    { 
                        success = false, 
                        message = "La fecha final debe ser mayor o igual a la fecha inicial" 
                    });
                }

                _logger.LogInformation("🔍 Buscando movimientos serie entre {FechaInicio:yyyy-MM-dd} y {FechaFin:yyyy-MM-dd}", 
                    fechaInicio, fechaFin);

                var movimientosSerie = await _service.GetByDateRangeAsync(fechaInicio, fechaFin);

                sw.Stop();
                _logger.LogInformation("✅ Búsqueda completada en {Elapsed}ms. Resultados: {Count}", 
                    sw.ElapsedMilliseconds, movimientosSerie.Count);

                return Ok(new
                {
                    success = true,
                    data = movimientosSerie,
                    total = movimientosSerie.Count,
                    dateRange = new
                    {
                        inicio = fechaInicio.ToString("yyyy-MM-dd"),
                        fin = fechaFin.ToString("yyyy-MM-dd")
                    },
                    message = movimientosSerie.Any() 
                        ? $"Se encontraron {movimientosSerie.Count} movimiento(s) serie entre {fechaInicio:yyyy-MM-dd} y {fechaFin:yyyy-MM-dd}" 
                        : $"No se encontraron movimientos serie en el rango de fechas especificado"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener movimientos serie por rango de fechas después de {Elapsed}ms", sw.ElapsedMilliseconds);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener los movimientos serie",
                    error = ex.Message
                });
            }
        }
    }
}
