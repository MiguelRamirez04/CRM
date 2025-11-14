using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.shared;
using back_cabs.CRM.models.Shared;
using Microsoft.EntityFrameworkCore;
using back_cabs.CRM.repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_cabs.CRM.Interfaces;

namespace back_cabs.CRM.services.shared
{
    public class EvaluacionDetallesService
    {
        // 1. ¡Inyectamos el REPOSITORIO, no los DbContext!
        private readonly IDetalleEvaluacionRepository _detalleRepository;

        public EvaluacionDetallesService(IDetalleEvaluacionRepository detalleRepository)
        {
            _detalleRepository = detalleRepository;
        }

        #region Queries

        public async Task<List<DtoEvaDetallesResponse>> GetDetallesByEvaluacionIdAsync(int evaluacionId)
        {
            // 1. Obtenemos las ENTIDADES del repositorio
            var detalles = await _detalleRepository.GetByEvaluacionIdAsync(evaluacionId);

            // 2. Mapeamos a DTOs (Esto se queda en el servicio)
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

        public async Task<DtoEvaDetallesResponse?> GetDetalleByIdAsync(int id)
        {
            // 1. Obtenemos la ENTIDAD del repositorio
            var detalle = await _detalleRepository.GetByIdAsync(id);

            if (detalle == null)
            {
                return null;
            }

            // 2. Mapeamos a DTO
            return new DtoEvaDetallesResponse
            {
                Id = detalle.Id,
                EvaluacionId = detalle.EvaluacionId,
                Fase = detalle.Fase,
                // ... resto de propiedades ...
                Lugar = detalle.Lugar
            };
        }

        #endregion

        #region Comandos

        public async Task<DtoEvaDetallesResponse> CreateDetalleAsync(DtoEvaDetallesRequest requestDto)
        {
            // 1. Mapeamos DTO a ENTIDAD
            var nuevoDetalle = new EvaluacionDetalle
            {
                EvaluacionId = requestDto.EvaluacionId,
                Fase = requestDto.Fase,
                Descripcion = requestDto.Descripcion,
                Sugerencias = requestDto.Sugerencias,
                ScoreFase = requestDto.ScoreFase,
                EvidenciaNota = requestDto.EvidenciaNota,
                Lugar = requestDto.Lugar,
                // 2. Lógica de negocio (se queda en el servicio)
                CreadoEn = DateTime.UtcNow 
            };

            // 3. Guardamos usando el repositorio
            var detalleCreado = await _detalleRepository.CreateAsync(nuevoDetalle);

            // 4. Mapeamos la entidad (ya con ID) de vuelta a un DTO
            return new DtoEvaDetallesResponse
            {
                Id = detalleCreado.Id,
                EvaluacionId = detalleCreado.EvaluacionId,
                // ... resto de propiedades ...
                CreadoEn = detalleCreado.CreadoEn
            };
        }

        public async Task<DtoEvaDetallesResponse?> UpdateDetalleAsync(int id, DtoEvaDetallesRequest requestDto)
        {
            // 1. Obtenemos la entidad existente
            var detalleExistente = await _detalleRepository.GetByIdAsync(id);

            if (detalleExistente == null)
            {
                return null; // No se encontró
            }

            // 2. Actualizamos la entidad con datos del DTO (Mapeo)
            detalleExistente.EvaluacionId = requestDto.EvaluacionId;
            detalleExistente.Fase = requestDto.Fase;
            detalleExistente.Descripcion = requestDto.Descripcion;
            detalleExistente.Sugerencias = requestDto.Sugerencias;
            detalleExistente.ScoreFase = requestDto.ScoreFase;
            detalleExistente.EvidenciaNota = requestDto.EvidenciaNota;
            detalleExistente.Lugar = requestDto.Lugar;
            // No actualizamos 'CreadoEn'

            // 3. Guardamos los cambios usando el repositorio
            await _detalleRepository.UpdateAsync(detalleExistente);

            // 4. Mapeamos la entidad actualizada a un DTO de respuesta
            //    (Reutilizamos el objeto 'detalleExistente' que ya está actualizado)
            return new DtoEvaDetallesResponse
            {
                Id = detalleExistente.Id,
                EvaluacionId = detalleExistente.EvaluacionId,
                Fase = detalleExistente.Fase,
                // ... resto de propiedades ...
            };
        }

        public async Task<bool> DeleteDetalleAsync(int id)
        {
            // El servicio solo delega la llamada
            return await _detalleRepository.DeleteAsync(id);
        }

        #endregion
    }
}