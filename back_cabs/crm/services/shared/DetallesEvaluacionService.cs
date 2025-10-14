using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.shared;
using back_cabs.CRM.models.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_cabs.CRM.services.shared
{
    public class EvaluacionDetallesService
    {
        private readonly ReadOnlyContext _readOnlyContext;
        private readonly WriteContext _writeContext;

        public EvaluacionDetallesService(ReadOnlyContext readOnlyContext, WriteContext writeContext)
        {
            _readOnlyContext = readOnlyContext;
            _writeContext = writeContext;
        }
        #region 
        /// <summary>
        /// Obtiene todos los detalles de una evaluación específica.
        /// </summary>
        /// <param name="evaluacionId">El ID de la evaluación padre.</param>
        /// <returns>Una lista de detalles de la evaluación.</returns>
        public async Task<List<DtoEvaDetallesResponse>> GetDetallesByEvaluacionIdAsync(int evaluacionId)
        {
            var detalles = await _readOnlyContext.EvaluacionesDetalles
                                    .Where(d => d.EvaluacionId == evaluacionId)
                                    .ToListAsync();

            // Mapeo de la entidad al DTO de respuesta
            return detalles.Select(detalle => new DtoEvaDetallesResponse
            {
                Id = detalle.Id,
                EvaluacionId = detalle.EvaluacionId,
                Fase = detalle.Fase,
                Descripcion = detalle.Descripcion,
                Sugerencias = detalle.Sugerencias,
                ScoreFase = detalle.ScoreFase,
                EvidenciasNota = detalle.EvidenciaNota,
                CreadoEn = detalle.CreadoEn,
                Lugar = detalle.Lugar
            }).ToList();
        }
        /// <summary>
        /// Obtiene un detalle de evaluación específico por su ID.
        /// </summary>
        /// <param name="id">El ID del detalle a buscar.</param>
        /// <returns>El DTO del detalle si se encuentra; de lo contrario, null.</returns>
        public async Task<DtoEvaDetallesResponse?> GetDetalleByIdAsync(int id)
        {
            var detalle = await _readOnlyContext.EvaluacionesDetalles.FindAsync(id);

            if (detalle == null)
            {
                return null;
            }

            // Mapeo de la entidad al DTO de respuesta
            return new DtoEvaDetallesResponse
            {
                Id = detalle.Id,
                EvaluacionId = detalle.EvaluacionId,
                Fase = detalle.Fase,
                Descripcion = detalle.Descripcion,
                Sugerencias = detalle.Sugerencias,
                ScoreFase = detalle.ScoreFase,
                EvidenciasNota = detalle.EvidenciaNota,
                CreadoEn = detalle.CreadoEn,
                Lugar = detalle.Lugar
            };
        }

        #endregion

        #region Operaciones de Escritura (Commands)

        /// <summary>
        /// Crea un nuevo detalle de evaluación.
        /// </summary>
        /// <param name="requestDto">Los datos para el nuevo detalle.</param>
        /// <returns>El DTO del detalle recién creado.</returns>
        public async Task<DtoEvaDetallesResponse> CreateDetalleAsync(DtoEvaDetallesRequest requestDto)
        {
            // 1. Mapear el DTO de petición a una nueva entidad del modelo
            var nuevoDetalle = new EvaluacionDetalle
            {
                EvaluacionId = requestDto.EvaluacionId,
                Fase = requestDto.Fase,
                Descripcion = requestDto.Descripcion,
                Sugerencias = requestDto.Sugerencias,
                ScoreFase = requestDto.ScoreFase,
                EvidenciaNota = requestDto.EvidenciaNota,
                Lugar = requestDto.Lugar,
                CreadoEn = DateTime.UtcNow // Lógica de negocio: El servidor establece la fecha de creación
            };

            // 2. Agregar la nueva entidad al contexto y guardar en la BD
            _writeContext.EvaluacionesDetalles.Add(nuevoDetalle);
            await _writeContext.SaveChangesAsync();

            // 3. Mapear la entidad recién creada (con su nuevo Id) a un DTO de respuesta
            return new DtoEvaDetallesResponse
            {
                Id = nuevoDetalle.Id,
                EvaluacionId = nuevoDetalle.EvaluacionId,
                Fase = nuevoDetalle.Fase,
                Descripcion = nuevoDetalle.Descripcion,
                Sugerencias = nuevoDetalle.Sugerencias,
                ScoreFase = nuevoDetalle.ScoreFase,
                EvidenciasNota = nuevoDetalle.EvidenciaNota,
                CreadoEn = nuevoDetalle.CreadoEn,
                Lugar = nuevoDetalle.Lugar
            };
        }
        
        /// <summary>
        /// Actualiza un detalle de evaluación existente.
        /// </summary>
        /// <param name="id">El ID del detalle a actualizar.</param>
        /// <param name="requestDto">Los nuevos datos para el detalle.</param>
        /// <returns>El DTO del detalle actualizado, o null si no se encontró.</returns>
        public async Task<DtoEvaDetallesResponse?> UpdateDetalleAsync(int id, DtoEvaDetallesRequest requestDto)
        {
            var detalleExistente = await _writeContext.EvaluacionesDetalles.FindAsync(id);

            if (detalleExistente == null)
            {
                return null; // No se encontró el recurso para actualizar
            }

            // Actualizar las propiedades de la entidad con los datos del DTO
            detalleExistente.EvaluacionId = requestDto.EvaluacionId; // Permitir cambiar la evaluación padre
            detalleExistente.Fase = requestDto.Fase;
            detalleExistente.Descripcion = requestDto.Descripcion;
            detalleExistente.Sugerencias = requestDto.Sugerencias;
            detalleExistente.ScoreFase = requestDto.ScoreFase;
            detalleExistente.EvidenciaNota = requestDto.EvidenciaNota;
            detalleExistente.Lugar = requestDto.Lugar;
            // No se actualiza 'CreadoEn'

            await _writeContext.SaveChangesAsync();

            return await GetDetalleByIdAsync(id); // Reutilizamos el método para obtener el DTO actualizado
        }

        /// <summary>
        /// Elimina un detalle de evaluación por su ID.
        /// </summary>
        /// <param name="id">El ID del detalle a eliminar.</param>
        /// <returns>True si se eliminó, False si no se encontró.</returns>
        public async Task<bool> DeleteDetalleAsync(int id)
        {
            var detalleAEliminar = await _writeContext.EvaluacionesDetalles.FindAsync(id);

            if (detalleAEliminar == null)
            {
                return false;
            }

            _writeContext.EvaluacionesDetalles.Remove(detalleAEliminar);
            await _writeContext.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}
