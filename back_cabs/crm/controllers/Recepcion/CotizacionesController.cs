using CRM.DTOs.Request;
using CRM.DTOs.Response;
using back_cabs.CRM.services.Recepcion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_cabs.CRM.Core.Exceptions;

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

<<<<<<< HEAD
    // [HttpGet("orden/{ordenId}")]
    // [ProducesResponseType(typeof(IEnumerable<CotizacionResponseDto>), 200)]
    // public async Task<IActionResult> GetByOrdenId(int ordenId)
    // {
    //     try
    //     {
    //         var resultado = await _service.ObtenerPorOrdenIdAsync(ordenId);
    //         return Ok(resultado);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error al obtener cotizaciones por OrdenId {OrdenId}.", ordenId);
    //         return StatusCode(500, new { message = "Error interno del servidor." });
    //     }
    // }
=======
    [HttpGet("orden/{ordenId}")]
    [ProducesResponseType(typeof(IEnumerable<CotizacionResponseDto>), 200)]
    /*
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
    }*/
>>>>>>> 29afbe45571ab99f1c722a38a504c27ea9e3be5c

    /// <summary>
    /// Obtiene cotizaciones filtradas por un campo de estado específico
    /// </summary>
    /// <param name="campo">Campo a filtrar: cancelado, afectado, impreso, usaCliente</param>
    /// <param name="valor">Valor del campo: 0 o 1</param>
    [HttpGet("estado/{campo}/{valor}")]
    [ProducesResponseType(typeof(IEnumerable<CotizacionResponseDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetByEstado(string campo, int valor)
    {
        try
        {
            if (valor != 0 && valor != 1)
                return BadRequest(new { message = "El valor debe ser 0 o 1." });

            var resultado = await _service.ObtenerPorEstadoAsync(campo, valor);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones por estado {Campo} = {Valor}.", campo, valor);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    /// <summary>
    /// Obtiene cotizaciones por ID de cliente
    /// </summary>
    [HttpGet("cliente/{clienteId:int}")]
    [ProducesResponseType(typeof(IEnumerable<CotizacionResponseDto>), 200)]
    public async Task<IActionResult> GetByClienteId(int clienteId)
    {
        try
        {
            var resultado = await _service.ObtenerPorClienteIdAsync(clienteId);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones por ClienteId {ClienteId}.", clienteId);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(CotizacionResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)] // Conflict para duplicados
    public async Task<IActionResult> Create([FromBody] CotizacionCreateRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _service.CrearAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = resultado.Id }, resultado);
        }
        catch (ForeignKeyNotFoundException ex)
        {
            // Excepción 1: Dato no corresponde a uno existente
            _logger.LogWarning(ex, "FK no encontrada al crear cotización: {Campo} = {ValorId}", ex.Campo, ex.ValorId);
            return BadRequest(new 
            { 
                error = "FOREIGN_KEY_NOT_FOUND",
                message = ex.Message,
                campo = ex.Campo,
                valorId = ex.ValorId
            });
        }
        catch (DuplicateRecordException ex)
        {
            // Excepción 2: Campo existente con duplicado
            _logger.LogWarning(ex, "Registro duplicado al crear cotización: {Campo} = {Valor}", ex.Campo, ex.Valor);
            return Conflict(new 
            { 
                error = "DUPLICATE_RECORD",
                message = ex.Message,
                campo = ex.Campo,
                valor = ex.Valor
            });
        }
        catch (ResourceAccessDeniedException ex)
        {
            // Excepción 3: Datos existentes pero no se puede acceder
            _logger.LogWarning(ex, "Acceso denegado al crear cotización");
            return StatusCode(403, new 
            { 
                error = "ACCESS_DENIED",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
<<<<<<< HEAD
            _logger.LogError(ex, "Error al crear cotización.");
=======
            _logger.LogError(ex, "Error inesperado al crear cotización");
>>>>>>> 29afbe45571ab99f1c722a38a504c27ea9e3be5c
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CotizacionResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] CotizacionCreateRequestDto request)
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