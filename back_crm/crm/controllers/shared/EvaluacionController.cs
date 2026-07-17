using back_cabs.CRM.DTOs.shared;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.services.shared;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

/// <summary>
/// agregar la cosa de los roles segun que tienes que debes de ver
/// </summary>
[ApiController] //esta es por asi decirlo la clase como control que voy a usar
[Route("api/[controller]")]
public class EvaluacionController : ControllerBase 
{
    private readonly EvaluacionService _evaluacionService;
    public EvaluacionController(EvaluacionService evaluacionService)
    {
        _evaluacionService = evaluacionService;
    }
    // --- EDPOINT (FUncionalidades) ---
    ///<summary>
    /// 
    ///<summary>
    [HttpGet]
    public async Task<ActionResult<List<EvaluacionResponseDto>>> GetAll()
    {
        var evaluaciones = await _evaluacionService.GetAllEvaluacionesAsync();
        return Ok(evaluaciones);
    }
    /// <summary>
    /// GET: api/Evaluacion/5
    /// Obtiene una evaluación específica por su ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EvaluacionResponseDto>> GetById(int id)
    {
        var evaluacion = await _evaluacionService.GetEvaluacionByIdAsync(id);
        if (evaluacion == null)
        {
            return NotFound();
        }
        return Ok(evaluacion);
    }
    /// <summary>
    /// POST: api/Evaluacion
    /// Crea una nueva evaluación.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EvaluacionResponseDto>> Create([FromBody] EvaluacionRequestDto requestDto)
    {
        // 7. El [ApiController] valida automáticamente el DTO. Si 'OrdenId' o 'EvaluadorId' faltan, devolverá un 400 Bad Request.
        var nuevaEvaluacion = await _evaluacionService.CreateEvaluacionAsync(requestDto);

        // 8. Devuelve un 201 Created, la URL para obtener el nuevo recurso, y el recurso creado.
        return CreatedAtAction(nameof(GetById), new { id = nuevaEvaluacion.Id }, nuevaEvaluacion);
    }
    
    /// <summary>
    /// PUT: api/Evaluacion/5
    /// Actualiza una evaluación existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EvaluacionRequestDto requestDto)
    {
        var evaluacionActualizada = await _evaluacionService.UpdateEvaluacionAsync(id, requestDto);
        if (evaluacionActualizada == null)
        {
            return NotFound();
        }
        return NoContent();
    }
    /// <summary>
    /// DELETE: api/Evaluacion/5
    /// Elimina una evaluación por su ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _evaluacionService.DeleteEvaluacionAsync(id);

        if (!resultado)
        {
            return NotFound(); // Si el servicio no encontró nada que borrar, devuelve 404.
        }

        return NoContent(); // Devuelve 204 NoContent para una eliminación exitosa.
    }
}
