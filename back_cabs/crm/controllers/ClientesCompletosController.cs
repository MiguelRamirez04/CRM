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
        /// Obtiene una lista de todos los clientes completos.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var clientes = await _service.GetAllAsync();
            return Ok(clientes);
        }

        /// <summary>
        /// Busca clientes por un término de búsqueda.
        /// </summary>
        /// <param name="q">Término de búsqueda para nombre, RFC, email o teléfono.</param>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            _logger.LogInformation("Iniciando búsqueda de clientes con query: {Query}", q);
            var clientes = await _service.SearchAsync(q);
            return Ok(clientes);
        }
    }
}
