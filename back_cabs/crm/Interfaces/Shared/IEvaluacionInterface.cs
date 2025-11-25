
using back_cabs.CRM.DTOs.shared;
using back_cabs.CRM.models.Shared;

namespace back_cabs.CRM.Interfaces
{
    public interface IEvaluacionINterface
    {
        Task<Evaluacion> GetaAllEvaluacionAsync();
        Task<Evaluacion> GetEvaluacionByIdAync(int id);
        Task<List<Evaluacion>> GetEvaluacionByOrdenIdAsync(int ordenId);
        Task<Evaluacion> CreateEvaluacionAsync(EvaluacionRequestDto evaluacion);
        Task<Evaluacion> UpdateEvalacionAsync(int id);
        Task<bool> DeleteEvaluacionAsync();

    }       
}   