///Esto es para los controles de evaluacion
/// using back_cabs.CRM.DTOs.shared;
using back_cabs.CRM.services.shared;
using Microsoft.AspNetCore.Mvc;
using back_cabs.CRM.DTOs.shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back_cabs.CRM.Controllers
{
    /// <summary>
    /// Controlador para gestionar los detalles de las evaluaciones.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluacionDetallesController : ControllerBase
    {
        private readonly EvaluacionDetallesService _evaluacionDetallesService;

        // Inyectamos el servicio en el constructor para poder usarlo.
        public EvaluacionDetallesController(EvaluacionDetallesService evaluacionDetallesService)
        {
            _evaluacionDetallesService = evaluacionDetallesService;
        }

        // --- ENDPOINTS (Acciones) ---

        /// <summary>
        /// Obtiene un detalle de evaluación específico por su ID.
        /// </summary>
        /// <param name="id">El ID del detalle a buscar.</param>
        /// <returns>El detalle de la evaluación.</returns>
        /// <response code="200">Retorna el detalle encontrado.</response>
        /// <response code="404">Si no se encuentra el detalle con ese ID.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DtoEvaDetallesResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DtoEvaDetallesResponse>> GetById(int id)
        {
            var detalle = await _evaluacionDetallesService.GetDetalleByIdAsync(id);

            if (detalle == null)
            {
                return NotFound($"No se encontró ningún detalle con el ID {id}.");
            }

            return Ok(detalle);
        }

        /// <summary>
        /// Obtiene todos los detalles asociados a una evaluación padre.
        /// </summary>
        /// <param name="evaluacionId">El ID de la evaluación principal.</param>
        /// <returns>Una lista de detalles.</returns>
        /// <response code="200">Retorna la lista de detalles.</response>
        [HttpGet("por-evaluacion/{evaluacionId}")]
        [ProducesResponseType(typeof(List<DtoEvaDetallesResponse>), 200)]
        public async Task<ActionResult<List<DtoEvaDetallesResponse>>> GetByEvaluacionId(int evaluacionId)
        {
            var detalles = await _evaluacionDetallesService.GetDetallesByEvaluacionIdAsync(evaluacionId);
            return Ok(detalles);
        }

        /// <summary>
        /// Crea un nuevo detalle de evaluación.
        /// </summary>
        /// <param name="requestDto">Los datos para el nuevo detalle.</param>
        /// <returns>El nuevo detalle creado.</returns>
        /// <response code="201">Retorna el nuevo detalle creado y la URL para acceder a él.</response>
        /// <response code="400">Si los datos enviados no son válidos.</response>
        [HttpPost]
        [ProducesResponseType(typeof(DtoEvaDetallesResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<DtoEvaDetallesResponse>> Create([FromBody] DtoEvaDetallesRequest requestDto)
        {
            // El [ApiController] ya valida el modelo (DTO) automáticamente.
            // Si falta un campo requerido, devolverá un 400 Bad Request.

            var nuevoDetalle = await _evaluacionDetallesService.CreateDetalleAsync(requestDto);

            // Devolvemos un 201 Created, la URL para obtener el nuevo recurso, y el recurso mismo.
            return CreatedAtAction(nameof(GetById), new { id = nuevoDetalle.Id }, nuevoDetalle);
        }

        /// <summary>
        /// Actualiza un detalle de evaluación existente.
        /// </summary>
        /// <param name="id">El ID del detalle a actualizar.</param>
        /// <param name="requestDto">Los nuevos datos para el detalle.</param>
        /// <returns>El detalle actualizado.</returns>
        /// <response code="200">Retorna el detalle con los datos actualizados.</response>
        /// <response code="404">Si no se encuentra el detalle a actualizar.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DtoEvaDetallesResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] DtoEvaDetallesRequest requestDto)
        {
            var detalleActualizado = await _evaluacionDetallesService.UpdateDetalleAsync(id, requestDto);

            if (detalleActualizado == null)
            {
                return NotFound($"No se pudo actualizar porque no se encontró el detalle con ID {id}.");
            }

            return Ok(detalleActualizado);
        }

        /// <summary>
        /// Elimina un detalle de evaluación por su ID.
        /// </summary>
        /// <param name="id">El ID del detalle a eliminar.</param>
        /// <response code="204">Si la eliminación fue exitosa.</response>
        /// <response code="404">Si no se encuentra el detalle a eliminar.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var resultado = await _evaluacionDetallesService.DeleteDetalleAsync(id);

            if (!resultado)
            {
                return NotFound($"No se pudo eliminar porque no se encontró el detalle con ID {id}.");
            }

            // Un 204 NoContent es la respuesta estándar para una eliminación exitosa.
            return NoContent();
        }
    }
}