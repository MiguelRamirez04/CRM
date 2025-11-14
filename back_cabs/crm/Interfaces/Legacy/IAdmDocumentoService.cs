using back_cabs.CRM.DTOs.Legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para el servicio de AdmDocumentos
    /// </summary>
    public interface IAdmDocumentoService
    {
        /// <summary>
        /// Busca documentos aplicando filtros con paginación
        /// </summary>
        Task<(List<AdmDocumentoResponseDto> documentos, int totalRegistros)> SearchPaginatedAsync(AdmDocumentoFilterDto filter);
        
        /// <summary>
        /// Obtiene un documento por ID incluyendo sus movimientos (productos cotizados)
        /// </summary>
        Task<AdmDocumentoResponseDto?> GetByIdWithMovimientosAsync(int idDocumento);
    }
}
