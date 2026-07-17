using CRM.DTOs.Request;
using CRM.DTOs.Response;
using back_cabs.CRM.services.Fleet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using back_cabs.CRM.services;

namespace back_cabs.CRM.controllers.Fleet;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiculosController : ControllerBase
{
    private readonly VehiculosService _service;
    private readonly ILogger<VehiculosController> _logger;
    private readonly ICacheService _cache;

    public VehiculosController(VehiculosService service, ILogger<VehiculosController> logger, ICacheService cache)
    {
        _service = service;
        _logger = logger;
        _cache = cache;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VehiculoResponseDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var key = "vehiculos:active";
        var cached = await _cache.GetAsync<IEnumerable<VehiculoResponseDto>>(key);
        if (cached != null) return Ok(cached);

        try
        {
            var resultado = await _service.ObtenerTodosAsync();
            await _cache.SetAsync(key, resultado, TimeSpan.FromMinutes(10));
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
        var key = $"vehiculos:id:{id}";
        var cached = await _cache.GetAsync<VehiculoResponseDto>(key);
        if (cached != null) return Ok(cached);

        try
        {
            var resultado = await _service.ObtenerPorIdAsync(id);
            if (resultado == null) return NotFound(new { message = $"Vehículo con ID {id} no encontrado." });

            await _cache.SetAsync(key, resultado, TimeSpan.FromMinutes(10));
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
            return StatusCode(201, resultado);
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
    public async Task<IActionResult> Update(int id, [FromBody] VehiculoUpdateDto request)
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

    [HttpGet("{id}/historial")]
    [ProducesResponseType(typeof(IEnumerable<VehiculoHistorialResponseDto>), 200)]
    public async Task<IActionResult> GetHistorial(int id)
    {
        try
        {
            var historial = await _service.ObtenerHistorialAsync(id);
            return Ok(historial);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial del vehículo {VehiculoId}", id);
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
    [HttpPost("{id}/salida")]
    [ProducesResponseType(typeof(VehiculoResponseDto), 200)]
    public async Task<IActionResult> RegistrarSalida(int id, [FromBody] RegistrarSalidaDto request)
    {
        try
        {
            var resultado = await _service.RegistrarSalidaAsync(id, request);
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar salida del vehículo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpPost("{id}/entrada")]
    [ProducesResponseType(typeof(VehiculoResponseDto), 200)]
    public async Task<IActionResult> RegistrarEntrada(int id, [FromBody] RegistrarEntradaDto request)
    {
        try
        {
            var resultado = await _service.RegistrarEntradaAsync(id, request);
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar entrada del vehículo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpGet("{id}/historial-uso")]
    public async Task<IActionResult> GetHistorialUso(int id)
    {
        var historial = await _service.ObtenerHistorialUsoAsync(id);
        return Ok(historial);
    }
}
