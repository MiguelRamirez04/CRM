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

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS POST (CREACIÓN)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea un nuevo documento en el sistema legacy
        /// Valida las relaciones y aplica reglas de negocio
        /// </summary>
        /// <param name="dto">DTO con los datos del documento a crear</param>
        /// <returns>El ID del documento creado</returns>
        Task<int> CreateAsync(AdmDocumentoCreateDto dto);

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS PARA COTIZACIONES MEJORADAS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea una nueva cotización de forma simplificada
        /// Aplica valores por defecto y calcula totales automáticamente
        /// </summary>
        /// <param name="dto">DTO simplificado de cotización</param>
        /// <returns>Respuesta con datos de la cotización creada</returns>
        Task<AdmCotizacionCreateResponseDto> CreateCotizacionAsync(AdmCotizacionCreateDto dto);

        /// <summary>
        /// Cancela una cotización existente
        /// Cambia el campo CCANCELADO a 1
        /// </summary>
        /// <param name="dto">DTO con datos de cancelación</param>
        /// <returns>Respuesta con datos de la cotización cancelada</returns>
        Task<AdmCotizacionCancelarResponseDto> CancelarCotizacionAsync(AdmCotizacionCancelarDto dto);

        /// <summary>
        /// Elimina una cotización (solo si está cancelada)
        /// </summary>
        Task DeleteAsync(int idDocumento);
    }
}
