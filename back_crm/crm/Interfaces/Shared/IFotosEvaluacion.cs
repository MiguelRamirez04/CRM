using back_cabs.CRM.DTOs.shared;
// No se necesita 'back_cabs.CRM.models.Shared' si la interfaz
// solo expone los DTOs, lo cual es una buena práctica.

namespace back_cabs.CRM.Interfaces
{
    public interface IFotosEvaluacion
    {
        /// <summary>
        /// Sube una foto para una evaluación
        /// </summary>
        Task<EvaluacionFotoResponseDto> CreateFotoAsync(
            EvaluacionFotoRequestDto dto, 
            int usuarioId);

        /// <summary>
        /// Obtiene todas las fotos de evaluación
        /// </summary>
        Task<List<EvaluacionFotoResponseDto>> GetAllFotosAsync();

        /// <summary>
        /// Obtiene fotos por detalle de evaluación
        /// </summary>
        Task<List<EvaluacionFotoResponseDto>> GetFotosByDetalleAsync(int detalleId);

        /// <summary>
        /// Obtiene una foto por su ID
        /// </summary>
        Task<EvaluacionFotoResponseDto?> GetFotoByIdAsync(int fotoId);

        /// <summary>
        /// Descarga el archivo de una foto
        /// </summary>
        Task<(byte[] FileBytes, string FileName, string MimeType)?> GetFotoFileAsync(int fotoId);

        /// <summary>
        /// Elimina una foto (soft delete del documento)
        /// </summary>
        Task<bool> DeleteFotoAsync(int fotoId, int usuarioId);
    }
}