using back_cabs.CRM.Interfaces.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace back_cabs.CRM.controllers.Legacy
{
    /// <summary>
    /// Controlador para AdmNumerosSerie (Legacy Adminpaq)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AdmNumerosSerieController : ControllerBase
    {
        private readonly IAdmNumeroSerieService _service;
        private readonly ILogger<AdmNumerosSerieController> _logger;

        public AdmNumerosSerieController(
            IAdmNumeroSerieService service,
            ILogger<AdmNumerosSerieController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Busca números de serie por producto y/o número de serie
        /// </summary>
        /// <param name="idProducto">ID del producto (opcional)</param>
        /// <param name="numeroSerie">Número de serie a buscar (opcional)</param>
        /// <returns>Lista de números de serie que coinciden con los criterios</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search(
            [FromQuery] int? idProducto,
            [FromQuery] string? numeroSerie)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                // Validar que al menos un parámetro esté presente
                if (!idProducto.HasValue && string.IsNullOrWhiteSpace(numeroSerie))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Debe proporcionar al menos un parámetro de búsqueda (idProducto o numeroSerie)"
                    });
                }

                // Validar longitud mínima del número de serie si se proporciona
                if (!string.IsNullOrWhiteSpace(numeroSerie) && numeroSerie.Trim().Length < 2)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El número de serie debe tener al menos 2 caracteres"
                    });
                }

                var result = await _service.SearchAsync(idProducto, numeroSerie);

                sw.Stop();
                _logger.LogInformation(
                    "✅ Búsqueda de números de serie completada en {Ms}ms. Total: {Total}",
                    sw.ElapsedMilliseconds, result.Count);

                return Ok(new
                {
                    success = true,
                    data = result,
                    total = result.Count,
                    ejecutionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "❌ Error en búsqueda de números de serie después de {Ms}ms",
                    sw.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al buscar números de serie",
                    error = ex.Message
                });
            }
        }
    }
}
