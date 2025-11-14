// =====================================================================================
// INTERFAZ REPOSITORY ADM PRODUCTO - IAdmProductoRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para acceso a datos de productos legacy (admProductos).
// Especifica métodos paginados para consultas y búsquedas por código/nombre.
//
// PROPÓSITO:
// - Abstracción de acceso a datos legacy
// - Paginación para grandes volúmenes de productos
// - Búsqueda eficiente por código y nombre de producto
//
// =====================================================================================

using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para repositorio de productos legacy (admProductos de adCABS2016)
    /// Proporciona acceso paginado a datos de productos del sistema Adminpaq
    /// </summary>
    public interface IAdmProductoRepository
    {
        /// <summary>
        /// Obtiene todos los productos con paginación
        /// </summary>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (default: 50)</param>
        /// <returns>Tupla con lista de productos y total de registros</returns>
        Task<(List<AdmProducto> Data, int TotalRecords)> GetAllPaginatedAsync(int page, int pageSize);

        /// <summary>
        /// Busca productos por código o nombre con paginación
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (código o nombre)</param>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (default: 50)</param>
        /// <returns>Tupla con lista de productos filtrados y total de registros</returns>
        Task<(List<AdmProducto> Data, int TotalRecords)> SearchPaginatedAsync(string searchTerm, int page, int pageSize);
    }
}