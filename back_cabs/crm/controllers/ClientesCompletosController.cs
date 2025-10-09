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
using CRM.DTOs.Response;

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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetClientesPaginados(
            [FromQuery] string? busqueda = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20)
        {
            try
            {
                // Validar parámetros
                if (pagina < 1)
                {
                    return BadRequest(new { mensaje = "El número de página debe ser mayor o igual a 1" });
                }
                
                if (porPagina < 1 || porPagina > 100)
                {
                    return BadRequest(new { mensaje = "El número de resultados por página debe estar entre 1 y 100" });
                }
                
                _logger.LogInformation("Obteniendo clientes completos con paginación. Página {Pagina}, Resultados por página {PorPagina}, Búsqueda general: {Busqueda}",
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
                return StatusCode(500, new { mensaje = "Error interno al procesar la solicitud. Detalles: " + ex.Message });
            }
        }

        /// <summary>
        /// Busca clientes por nombre comercial de forma paginada.
        /// </summary>
        /// <param name="nombre">Nombre comercial o parte del nombre para la búsqueda</param>
        /// <param name="pagina">Número de página (comienza en 1)</param>
        /// <param name="porPagina">Cantidad de resultados por página</param>
        [HttpGet("por-nombre")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetClientesPorNombre(
            [FromQuery] string? nombre = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20)
        {
            try
            {
                // Validar parámetros
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    _logger.LogWarning("Se intentó buscar por nombre sin proporcionar un valor");
                    return BadRequest(new { mensaje = "Debe proporcionar un nombre para la búsqueda" });
                }
                
                if (pagina < 1)
                {
                    return BadRequest(new { mensaje = "El número de página debe ser mayor o igual a 1" });
                }
                
                if (porPagina < 1 || porPagina > 100)
                {
                    return BadRequest(new { mensaje = "El número de resultados por página debe estar entre 1 y 100" });
                }
                
                _logger.LogInformation("Buscando clientes por nombre: {Nombre}, Página {Pagina}, Resultados por página {PorPagina}",
                    nombre, pagina, porPagina);

                var request = new ClientesCompletosPaginadoRequestDto
                {
                    NombreBusqueda = nombre,
                    Pagina = pagina,
                    ResultadosPorPagina = porPagina
                };

                var resultado = await _service.GetClientesPaginadosAsync(request);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar clientes por nombre: {Message}", ex.Message);
                return StatusCode(500, new { mensaje = "Error interno al procesar la solicitud" });
            }
        }

        /// <summary>
        /// Busca clientes por RFC de forma paginada.
        /// </summary>
        /// <param name="rfc">RFC o parte del RFC para la búsqueda</param>
        /// <param name="pagina">Número de página (comienza en 1)</param>
        /// <param name="porPagina">Cantidad de resultados por página</param>
        [HttpGet("por-rfc")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetClientesPorRfc(
            [FromQuery] string? rfc = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20)
        {
            try
            {
                // Validar parámetros
                if (string.IsNullOrWhiteSpace(rfc))
                {
                    _logger.LogWarning("Se intentó buscar por RFC sin proporcionar un valor");
                    return BadRequest(new { mensaje = "Debe proporcionar un RFC para la búsqueda" });
                }
                
                if (pagina < 1)
                {
                    return BadRequest(new { mensaje = "El número de página debe ser mayor o igual a 1" });
                }
                
                if (porPagina < 1 || porPagina > 100)
                {
                    return BadRequest(new { mensaje = "El número de resultados por página debe estar entre 1 y 100" });
                }
                
                _logger.LogInformation("Buscando clientes por RFC: {RFC}, Página {Pagina}, Resultados por página {PorPagina}",
                    rfc, pagina, porPagina);

                var request = new ClientesCompletosPaginadoRequestDto
                {
                    RFCBusqueda = rfc,
                    Pagina = pagina,
                    ResultadosPorPagina = porPagina
                };

                var resultado = await _service.GetClientesPaginadosAsync(request);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar clientes por RFC: {Message}", ex.Message);
                return StatusCode(500, new { mensaje = "Error interno al procesar la solicitud" });
            }
        }
        
        /// <summary>
        /// Búsqueda avanzada de clientes que permite filtrar por nombre y RFC simultáneamente.
        /// </summary>
        /// <param name="nombre">Nombre comercial o parte del nombre</param>
        /// <param name="rfc">RFC o parte del RFC</param>
        /// <param name="pagina">Número de página (comienza en 1)</param>
        /// <param name="porPagina">Cantidad de resultados por página</param>
        [HttpGet("busqueda-avanzada")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BusquedaAvanzada(
            [FromQuery] string? nombre = null,
            [FromQuery] string? rfc = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20)
        {
            try
            {
                // Validar parámetros
                if (string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(rfc))
                {
                    _logger.LogWarning("Se intentó realizar búsqueda avanzada sin proporcionar criterios");
                    return BadRequest(new { mensaje = "Debe proporcionar al menos un criterio de búsqueda (nombre o RFC)" });
                }
                
                if (pagina < 1)
                {
                    return BadRequest(new { mensaje = "El número de página debe ser mayor o igual a 1" });
                }
                
                if (porPagina < 1 || porPagina > 100)
                {
                    return BadRequest(new { mensaje = "El número de resultados por página debe estar entre 1 y 100" });
                }
                
                _logger.LogInformation("Realizando búsqueda avanzada de clientes. Nombre: {Nombre}, RFC: {RFC}, Página {Pagina}, Resultados por página {PorPagina}",
                    nombre ?? "no especificado", rfc ?? "no especificado", pagina, porPagina);

                var request = new ClientesCompletosPaginadoRequestDto
                {
                    NombreBusqueda = nombre,
                    RFCBusqueda = rfc,
                    Pagina = pagina,
                    ResultadosPorPagina = porPagina
                };

                var resultado = await _service.GetClientesPaginadosAsync(request);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en la búsqueda avanzada de clientes: {Message}", ex.Message);
                return StatusCode(500, new { mensaje = "Error interno al procesar la solicitud. Detalles: " + ex.Message });
            }
        }
    }
}
