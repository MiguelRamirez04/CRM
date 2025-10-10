using back_cabs.CRM.contexts;
using back_cabs.CRM.models.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back_cabs.CRM.services.shared
{
    public class EvaluacionService : Evaluacion
    {
        private readonly ReadOnlyContext _readOnlyContext;
        private readonly WriteContext _writeContext;

        public EvaluacionService(ReadOnlyContext readOnlyContext, WriteContext writeContext)
        {
            _readOnlyContext = readOnlyContext;
            _writeContext = writeContext;
        }
        #region OPeracion de lectura (Queries)
        ///<summary>
        /// Obtiene todas las operacoines de la base de datos.
        ///<summary>
        public async Task<List<Evaluacion>> GetAllEvaluacionesAsync()
        {
            //se usa _read only para mayor rendimiento en consultas esa es la sintaxis
            return await _readOnlyContext.Evaluaciones.ToListAsync();
        }
        /// <summary>
        /// Busca una evaluación específica por su ID.
        /// </summary>
        /// <param name="id">El ID de la evaluación a buscar.</param>
        /// <returns>La entidad Evaluacion si se encuentra, o null si no.</returns
        public async Task<Evaluacion?> GetEvaluacionByIsAsync(int id)
        {
            return await _readOnlyContext.Evaluaciones.FindAsync(id);
        }
        public async Task<List<Evaluacion>> GetEvaluacionByOrdenIdAsync(int ordenId)
        {
            return await _readOnlyContext.Evaluaciones
            .Where(e => e.OrdenId == ordenId)
            .ToListAsync();
        }

        #endregion

        #region Operaciones de escritura (Comandos)
        /// <summary>
        /// Crea una nueva evaluación en la base de datos.
        /// </summary>
        /// <param name="evaluacion">El objeto de evaluación con los datos a crear.</param>
        /// <returns>La entidad Evaluacion creada, con su nuevo ID asignado por la BD.</returns>
        public async Task<Evaluacion> CreateEvaluacionAsync(Evaluacion evaluacion)
        //console.writeline() esta wea es para la muesrta de la ibnfocuanbdo lavya a leerr 
        {
            //logica de negocion que se asegura de que la fecha y la hora  sean a la que  ah realizado
            evaluacion.CreadoEn = DateTime.UtcNow;
            _writeContext.Evaluaciones.Add(evaluacion);
            await _writeContext.SaveChangesAsync();
            return evaluacion;
        }

        /// <summary>
        /// Actualiza una evaluación existente.
        /// </summary>
        /// <param name="id">El ID de la evaluación a actualizar.</param>
        /// <param name="evaluacionActualizada">El objeto con los nuevos datos.</param>
        /// <returns>La entidad actualizada, o null si no se encontró la evaluación.</returns>
        public async Task<Evaluacion?> UpdateEvaluacionAsync(int id, Evaluacion evaluacionActualizada)
        {
            // Primero, se busca la entidad existente que se quiere modificar.
            var evaluacionExistente = await _writeContext.Evaluaciones.FindAsync(id);

            if (evaluacionExistente == null)
            {
                return null; // No se encontró la evaluación, no se puede actualizar.
            }

            evaluacionExistente.EjecucionId = evaluacionActualizada.EjecucionId;
            evaluacionExistente.ClienteId = evaluacionActualizada.ClienteId;
            evaluacionExistente.EvaluadorId = evaluacionActualizada.EvaluadorId;
            evaluacionExistente.Objetivo = evaluacionActualizada.Objetivo;
            evaluacionExistente.ComentariosGenerales = evaluacionActualizada.ComentariosGenerales;
            evaluacionExistente.ScoreCalidadTotal = evaluacionActualizada.ScoreCalidadTotal;
            evaluacionExistente.RequiereSeguimiento = evaluacionActualizada.RequiereSeguimiento;
            evaluacionExistente.SeguimientoNotas = evaluacionActualizada.SeguimientoNotas;
            await _writeContext.SaveChangesAsync();
            return evaluacionExistente;
        }
        /// <summary>
        /// Elimina una evaluación de la base de datos.
        /// </summary>
        /// <param name="id">El ID de la evaluación a eliminar.</param>
        /// <returns>True si se eliminó, False si no se encontró.</returns>
        public async Task<bool> DeleteEvaluacionAsync(int id)
        {
            var evaliacionAEliminar = await _writeContext.Evaluaciones.FindAsync(id);
            if (evaliacionAEliminar == null)
            {
                return false;
            }
            _writeContext.Evaluaciones.Remove(evaliacionAEliminar);
            await _writeContext.SaveChangesAsync();
            return true;
        }

        #endregion

    }
}