// =====================================================================================
// CONTROLADOR CLIENTES COMPLETOS - ClientesCompletosController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Expone los endpoints de la API para consultar la vista VwClientesCompletos.
// Define las rutas, maneja las solicitudes HTTP y utiliza ClientesCompletosService
// para obtener los datos.
//
// =====================================================================================

using back_cabs.CRM.services;
using Microsoft.AspNetCore.Mvc;
using back_cabs.CRM.DTOs;

namespace back_cabs.CRM.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesCompletosController : ControllerBase
    {
        private readonly ClientesCompletosService _service;
        private readonly ILogger<ClientesCompletosController> _logger;

        public ClientesCompletosController(ClientesCompletosService service, ILogger<ClientesCompletosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una lista paginada de clientes completos.
        /// </summary>
        /// <param name="busqueda">Término de búsqueda opcional para filtrar por nombre</param>
        /// <param name="pagina">Número de página (comienza en 1)</param>
        /// <param name="porPagina">Cantidad de resultados por página</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetClientesPaginados(
            [FromQuery] string? busqueda = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20)
        {
            try
            {
                _logger.LogInformation("Obteniendo clientes completos con paginación. Página {Pagina}, Resultados por página {PorPagina}, Búsqueda: {Busqueda}",
                    pagina, porPagina, busqueda ?? "ninguna");

                var request = new ClientesCompletosPaginadoRequestDto
                {
                    NombreBusqueda = busqueda,
                    Pagina = pagina,
                    ResultadosPorPagina = porPagina
                };

                var resultado = await _service.GetClientesPaginadosAsync(request);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes: {Message}", ex.Message);
                return StatusCode(500, new { mensaje = "Error interno al procesar la solicitud" });
            }
        }
    }
}
