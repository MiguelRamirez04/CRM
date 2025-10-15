using back_cabs.CRM.DTOs.shared;
using back_cabs.CRM.services.shared;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
/// <summary>
/// agregar la cosa de los roles segun que tienes que debes de ver
/// </summary>

[ApiController]
[Route("api/[Controller]")]
public class FotosEvaluacionController : ControllerBase
{
    private readonly FotosEvaluacionService _fotosService;

    // 1. Inyección de Dependencias
    public FotosEvaluacionController(FotosEvaluacionService fotosService)
    {
        _fotosService = fotosService;
    }

    // --- Endpoints ---

    /// <summary>
    /// GET: api/FotosEvaluacion
    /// Obtiene una lista de todas las fotos de evaluación.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<EvaluacionFotoResponseDto>>> GetAll()
    {
        var fotos = await _fotosService.GetAllFotosAsync();
        return Ok(fotos);
    }
    /// <summary>
    /// GET: api/FotosEvaluacion/5
    /// Obtiene una foto de evaluación específica por su ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EvaluacionFotoResponseDto>> GetById(int id)
    {
        var foto = await _fotosService.GetFotoByIdAsync(id);

        if (foto == null)
        {
            return NotFound(); //rrspuesta esperada de 404 no encontrada
        }
        return Ok(foto);
    }
    /// <summary>
    /// POST: api/FotosEvaluacion
    /// Crea una nueva foto de evaluación.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EvaluacionFotoResponseDto>> Create([FromBody] EvaluacionFotoRequestDto requestDto)
    {
        var nuevaFotoModel = await _fotosService.CreateFotoAsync(requestDto);
        var responseDto = new EvaluacionFotoResponseDto
        {
            Id = nuevaFotoModel.Id,
            DetalleId = nuevaFotoModel.DetalleId,
            DocumentoId = nuevaFotoModel.DocumentoId,
            Tipo = nuevaFotoModel.Tipo,
            Descripcion = nuevaFotoModel.Descripcion,
            CreadoEn = nuevaFotoModel.CreadoEn,
        };
        return CreatedAtAction(nameof(GetById), new { id = nuevaFotoModel.Id }, responseDto);

    }


    /// <summary>
    /// PUT: api/FotosEvaluacion/5
    /// Actualiza una foto de evaluación existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EvaluacionFotoRequestDto requestDto)
    {
        var fotoActualizada = await _fotosService.UpdateFotoAsync(id, requestDto);
        if (fotoActualizada == null)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// DELETE: api/FotosEvaluacion/5
    /// Elimina una foto de evaluación por su ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _fotosService.DeleteFotoAsync(id);
        if (!resultado)
        {
            return NotFound();
        }
        return NoContent();
    }
}