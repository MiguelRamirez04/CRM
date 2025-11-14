using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para el repositorio de AdmDocumentos
    /// </summary>
    public interface IAdmDocumentoRepository
    {
        /// <summary>
        /// Busca documentos aplicando filtros con paginación
        /// </summary>
        Task<(List<AdmDocumento> documentos, int totalRegistros)> SearchPaginatedAsync(AdmDocumentoFilterDto filter);
        
        /// <summary>
        /// Obtiene un documento por ID incluyendo sus movimientos
        /// </summary>
        Task<AdmDocumento?> GetByIdWithMovimientosAsync(int idDocumento);
        
        /// <summary>
        /// Obtiene los movimientos de un documento específico
        /// </summary>
        Task<List<AdmMovimiento>> GetMovimientosByDocumentoIdAsync(int idDocumento);
    }
}
