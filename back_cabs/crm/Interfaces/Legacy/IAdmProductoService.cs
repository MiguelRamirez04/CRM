// =====================================================================================
// INTERFAZ SERVICE ADM PRODUCTO - IAdmProductoService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de servicios de negocio para productos legacy.
// Incluye cache Redis y paginación robusta.
//
// PROPÓSITO:
// - Lógica de negocio para productos legacy
// - Cache Redis con Cache-Aside pattern
// - Paginación y búsqueda optimizadas
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para servicio de productos legacy con cache Redis
    /// Proporciona operaciones de negocio para admProductos de adCABS2016
    /// </summary>
    public interface IAdmProductoService
    {
        /// <summary>
        /// Obtiene todos los productos con paginación y cache
        /// Cache TTL: 30 minutos por página
        /// </summary>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (default: 50)</param>
        /// <returns>Respuesta paginada con productos y metadatos</returns>
        Task<PaginatedResponseDto<AdmProductoResponseDto>> GetAllPaginatedAsync(int page, int pageSize);

        /// <summary>
        /// Busca productos por código o nombre con paginación y cache
        /// Cache TTL: 60 minutos por término de búsqueda y página
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (código o nombre)</param>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (default: 50)</param>
        /// <returns>Respuesta paginada con productos filtrados y metadatos</returns>
        Task<PaginatedResponseDto<AdmProductoResponseDto>> SearchPaginatedAsync(string searchTerm, int page, int pageSize);
    }
}