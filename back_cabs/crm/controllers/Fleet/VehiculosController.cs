using CRM.DTOs.Request;
using CRM.DTOs.Response;
using back_cabs.CRM.services.Fleet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_cabs.CRM.controllers.Fleet;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiculosController : ControllerBase
{
    private readonly VehiculosService _service;
    private readonly ILogger<VehiculosController> _logger;

    public VehiculosController(VehiculosService service, ILogger<VehiculosController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VehiculoResponseDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var resultado = await _service.ObtenerTodosAsync();
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los vehículos.");
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VehiculoResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var resultado = await _service.ObtenerPorIdAsync(id);
            if (resultado == null)
                return NotFound(new { message = $"Vehículo con ID {id} no encontrado." });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener vehículo con ID {VehiculoId}", id);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(VehiculoResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] VehiculoRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        try
        {
            var resultado = await _service.CrearAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = resultado.Id }, resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el vehículo.");
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(VehiculoResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] VehiculoRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var resultado = await _service.ActualizarAsync(id, request);
            if (resultado == null)
                return NotFound(new { message = $"Vehículo con ID {id} no encontrado." });

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el vehículo con ID {VehiculoId}", id);
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
                return NotFound(new { message = $"Vehículo con ID {id} no encontrado." });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el vehículo con ID {VehiculoId}", id);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }
}
