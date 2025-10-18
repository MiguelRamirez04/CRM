using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using back_cabs.CRM.DTOs.shared;
using back_cabs.CRM.services.shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[Controller]")]
[Authorize(Roles = "SOPORTE,ADMINISTRACION")]
public class FotosEvaluacionController : ControllerBase
{
    private readonly FotosEvaluacionService _fotosService;
    private readonly ILogger<FotosEvaluacionController> _logger;

    public FotosEvaluacionController(
        FotosEvaluacionService fotosService, 
        ILogger<FotosEvaluacionController> logger)
    {
        _fotosService = fotosService ?? throw new ArgumentNullException(nameof(fotosService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    /// Sube una nueva foto de evaluación (convierte automáticamente a WebP).
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<EvaluacionFotoResponseDto>> Create([FromForm] EvaluacionFotoRequestDto requestDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var usuarioId = GetCurrentUserId();
            var responseDto = await _fotosService.CreateFotoAsync(requestDto, usuarioId);
            return CreatedAtAction(nameof(GetById), new { id = responseDto.Id }, responseDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir foto");
            return StatusCode(500, new { Error = "Error interno al subir foto." });
        }
    }

    /// <summary>
    /// GET: api/FotosEvaluacion/5/download
    /// Descarga una foto específica.
    /// </summary>
    [HttpGet("{id}/download")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFoto(int id)
    {
        try
        {
            _logger.LogInformation("Descargando foto {FotoId}", id);
            
            var result = await _fotosService.GetFotoFileAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Foto {FotoId} no encontrada", id);
                return NotFound(new { Error = "Foto no encontrada." });
            }

            return File(result.Value.FileBytes, result.Value.MimeType, result.Value.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar foto {FotoId}", id);
            return StatusCode(500, new { Error = "Error al descargar foto." });
        }
    }

    /// <summary>
    /// DELETE: api/FotosEvaluacion/5
    /// Elimina una foto de evaluación por su ID (archivo físico + BD).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var usuarioId = GetCurrentUserId();
            var resultado = await _fotosService.DeleteFotoAsync(id, usuarioId);
            if (!resultado)
            {
                return NotFound(new { Error = "Foto no encontrada." });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar foto {FotoId}", id);
            return StatusCode(500, new { Error = "Error al eliminar foto." });
        }
    }

    /// <summary>
    /// Obtiene el ID del usuario actual desde el token JWT.
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogError("No se pudo obtener el ID del usuario del token JWT");
            throw new UnauthorizedAccessException("Usuario no autenticado.");
        }
        return userId;
    }
}