// Asumo que tu entidad se llama "EvaluacionDetalle"
using back_cabs.CRM.models.Shared; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back_cabs.CRM.Interfaces // O tu namespace preferido para repositorios
{
    public interface IDetalleEvaluacionRepository
    {
        // Devuelve la ENTIDAD, no el DTO
        Task<List<EvaluacionDetalle>> GetByEvaluacionIdAsync(int evaluacionId);
        
        // Devuelve la ENTIDAD (puede ser nulable)
        Task<EvaluacionDetalle?> GetByIdAsync(int id);

        // Recibe la ENTIDAD para crear
        Task<EvaluacionDetalle> CreateAsync(EvaluacionDetalle detalle);

        // Recibe la ENTIDAD actualizada para guardar
        Task UpdateAsync(EvaluacionDetalle detalle);

        // Devuelve bool si se eliminó o no
        Task<bool> DeleteAsync(int id);
    }
}