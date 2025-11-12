using CRM.DTOs.Request;
using CRM.DTOs.Response;
using back_cabs.CRM.services.Recepcion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_cabs.CRM.controllers.Recepcion;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CotizacionesController : ControllerBase
{
    private readonly CotizacionService _service;
    private readonly ILogger<CotizacionesController> _logger;

    public CotizacionesController(CotizacionService service, ILogger<CotizacionesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CotizacionResponseDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var resultado = await _service.ObtenerTodosAsync();
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las cotizaciones.");
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CotizacionResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var resultado = await _service.ObtenerPorIdAsync(id);
            if (resultado == null)
                return NotFound(new { message = $"Cotización con ID {id} no encontrada." });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotización por ID {Id}.", id);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpGet("orden/{ordenId}")]
    [ProducesResponseType(typeof(IEnumerable<CotizacionResponseDto>), 200)]
    public async Task<IActionResult> GetByOrdenId(int ordenId)
    {
        try
        {
            var resultado = await _service.ObtenerPorOrdenIdAsync(ordenId);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones por OrdenId {OrdenId}.", ordenId);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpGet("estado/{estado}")]
    [ProducesResponseType(typeof(IEnumerable<CotizacionResponseDto>), 200)]
    public async Task<IActionResult> GetByEstado(string estado)
    {
        try
        {
            var resultado = await _service.ObtenerPorEstadoAsync(estado);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones por estado {Estado}.", estado);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpGet("cliente/{cliente}")]
    [ProducesResponseType(typeof(IEnumerable<CotizacionResponseDto>), 200)]
    public async Task<IActionResult> GetByCliente(string cliente)
    {
        try
        {
            var resultado = await _service.ObtenerPorClienteAsync(cliente);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones por cliente {Cliente}.", cliente);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(CotizacionResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CotizacionRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _service.CrearAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cotización.");
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CotizacionResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] CotizacionRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _service.ActualizarAsync(id, request);
            if (resultado == null)
                return NotFound(new { message = $"Cotización con ID {id} no encontrada." });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cotización {Id}.", id);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var eliminado = await _service.EliminarAsync(id);
            if (!eliminado)
                return NotFound(new { message = $"Cotización con ID {id} no encontrada." });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cotización {Id}.", id);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }
}