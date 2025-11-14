using back_cabs.CRM.Interfaces.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace back_cabs.CRM.controllers.Legacy
{
    /// <summary>
    /// Controlador para gestión de unidades de medida y peso
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AdmUnidadesMedidaPesoController : ControllerBase
    {
        private readonly IAdmUnidadMedidaPesoService _service;
        private readonly ILogger<AdmUnidadesMedidaPesoController> _logger;

        public AdmUnidadesMedidaPesoController(
            IAdmUnidadMedidaPesoService service,
            ILogger<AdmUnidadesMedidaPesoController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las unidades de medida y peso
        /// </summary>
        /// <returns>Lista completa de unidades (máximo 8 registros)</returns>
        /// <response code="200">Lista de unidades obtenida exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("📋 Obteniendo todas las unidades de medida");

                var unidades = await _service.GetAllAsync();

                sw.Stop();
                _logger.LogInformation("✅ Unidades obtenidas exitosamente en {Elapsed}ms. Total: {Count}", 
                    sw.ElapsedMilliseconds, unidades.Count);

                return Ok(new
                {
                    success = true,
                    data = unidades,
                    total = unidades.Count,
                    message = $"Se encontraron {unidades.Count} unidad(es) de medida"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener unidades después de {Elapsed}ms", sw.ElapsedMilliseconds);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener las unidades de medida",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener unidad de medida por ID
        /// </summary>
        /// <param name="id">ID de la unidad</param>
        /// <returns>Datos de la unidad solicitada</returns>
        /// <response code="200">Unidad encontrada</response>
        /// <response code="404">Unidad no encontrada</response>
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
                _logger.LogInformation("🔍 Buscando unidad de medida ID: {Id}", id);

                var unidad = await _service.GetByIdAsync(id);

                if (unidad == null)
                {
                    sw.Stop();
                    _logger.LogWarning("⚠️ Unidad {Id} no encontrada después de {Elapsed}ms", id, sw.ElapsedMilliseconds);
                    
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontró la unidad de medida con ID {id}"
                    });
                }

                sw.Stop();
                _logger.LogInformation("✅ Unidad {Id} encontrada en {Elapsed}ms", id, sw.ElapsedMilliseconds);

                return Ok(new
                {
                    success = true,
                    data = unidad,
                    message = "Unidad de medida encontrada"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener unidad {Id} después de {Elapsed}ms", id, sw.ElapsedMilliseconds);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener la unidad de medida",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Buscar unidades de medida por nombre
        /// </summary>
        /// <param name="nombre">Nombre o parte del nombre de la unidad (ej: "Kilo", "Pie")</param>
        /// <returns>Lista de unidades que coinciden con la búsqueda</returns>
        /// <response code="200">Búsqueda completada exitosamente</response>
        /// <response code="400">Parámetro de búsqueda inválido</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchByName([FromQuery] string nombre)
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    _logger.LogWarning("⚠️ Búsqueda sin parámetro 'nombre', devolviendo todas las unidades");
                    return await GetAll();
                }

                _logger.LogInformation("🔍 Buscando unidades por nombre: '{Nombre}'", nombre);

                var unidades = await _service.SearchByNameAsync(nombre);

                sw.Stop();
                _logger.LogInformation("✅ Búsqueda completada en {Elapsed}ms. Resultados: {Count}", 
                    sw.ElapsedMilliseconds, unidades.Count);

                return Ok(new
                {
                    success = true,
                    data = unidades,
                    total = unidades.Count,
                    searchTerm = nombre,
                    message = unidades.Any() 
                        ? $"Se encontraron {unidades.Count} unidad(es) que coinciden con '{nombre}'" 
                        : $"No se encontraron unidades que coincidan con '{nombre}'"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al buscar unidades por nombre '{Nombre}' después de {Elapsed}ms", 
                    nombre, sw.ElapsedMilliseconds);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al buscar unidades de medida",
                    error = ex.Message
                });
            }
        }
    }
}
