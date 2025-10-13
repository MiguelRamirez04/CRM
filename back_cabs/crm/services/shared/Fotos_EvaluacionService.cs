using back_cabs.CRM.contexts;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.DTOs.shared;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.services.shared
{
    public class FotosEvaluacionService : EvaluacionFoto
    {
        private readonly ReadOnlyContext _readonlyContext;
        private readonly WriteContext _writeContext;
        public FotosEvaluacionService(ReadOnlyContext readOnlyContext, WriteContext writeContext)
        {
            _readonlyContext = readOnlyContext;
            _writeContext = writeContext;
        }
        #region Operacion de lectrua (Queries)

        ///<summary>
        /// Obtiene todas las operacoines de la base de datos.
        ///<summary>
        /// Pregunta estos comentarios son necesarios?
        public async Task<List<EvaluacionFotoResponseDto>> GetAllFotosAsync()
        {
            return await _readonlyContext.EvaluacionesFotos
                .Select(foto => new EvaluacionFotoResponseDto
                {
                    Id = foto.Id,
                    DetalleId = foto.DetalleId,
                    DocumentoId = foto.DocumentoId,
                    Tipo = foto.Tipo,
                    Descripcion = foto.Descripcion,
                    CreadoEn = foto.CreadoEn,
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una foto de evaluación por su ID.
        /// </summary>
        public async Task<EvaluacionFotoResponseDto?> GetFotoByIdAsync(int id)
        {
            var foto = await _readonlyContext.EvaluacionesFotos.FindAsync(id);
            if (foto == null)
            {
                return null;
            }

            return new EvaluacionFotoResponseDto
            {
                Id = foto.Id,
                DetalleId = foto.DetalleId,
                DocumentoId = foto.DocumentoId,
                Tipo = foto.Tipo,
                Descripcion = foto.Descripcion,
                CreadoEn = foto.CreadoEn,
            };
        }

        #endregion

        #region Operaciones de escritura (Commands)

        /// <summary>
        /// Crea una nueva entrada de foto de evaluación.
        /// </summary>
        /// 
        public async Task<EvaluacionFoto> CreateFotoAsync(EvaluacionFotoRequestDto requestDto)
        {
            var nuevaFoto = new EvaluacionFoto
            {
                DetalleId = requestDto.DetalleId,
                DocumentoId = requestDto.DocumentoId,
                Tipo = requestDto.Tipo,
                Descripcion = requestDto.Descripcion,
                CreadoEn = requestDto.CreadoEn,
            };
            _writeContext.EvaluacionesFotos.Add(nuevaFoto);
            await _writeContext.SaveChangesAsync();
            return nuevaFoto;
        }

        /// <summary>
        /// Actualiza una foto de evaluación existente.
        /// </summary>
        public async Task<EvaluacionFoto?> UpdateFotoAsync(int id, EvaluacionFotoRequestDto requestDto)
        {
            var fotoExistente = await _writeContext.EvaluacionesFotos.FindAsync(id);

            if (fotoExistente == null)
            {
                return null;
            }
            fotoExistente.DetalleId = requestDto.DetalleId;
            fotoExistente.DocumentoId = requestDto.DocumentoId;
            fotoExistente.Tipo = requestDto.Tipo;
            fotoExistente.Descripcion = requestDto.Descripcion;
            fotoExistente.CreadoEn = requestDto.CreadoEn;

            await _writeContext.SaveChangesAsync();
            return fotoExistente;
        }

        /// <summary>
        /// Elimina una foto de evaluación por su ID.
        /// </summary>
        public async Task<bool> DeleteFotoAsync(int id)
        {
            var fotoAEliminar = await _writeContext.EvaluacionesFotos.FindAsync(id);

            if (fotoAEliminar == null)
            {
                return false;
            }

            _writeContext.EvaluacionesFotos.Remove(fotoAEliminar);
            await _writeContext.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}
