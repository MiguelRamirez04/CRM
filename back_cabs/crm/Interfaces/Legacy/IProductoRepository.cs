// =====================================================================================
// INTERFACE PRODUCTO REPOSITORY - IProductoRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para operaciones de acceso a datos de productos.
// Sigue el patrón Repository para abstraer la lógica de persistencia.
//
// MÉTODOS:
// - GetAllProductosAsync: Obtiene todos los productos activos
// - SearchProductosByFilterAsync: Busca productos por código o nombre
//
// USO:
// - Implementado por ProductoRepository
// - Inyectado en ProductoService
// - Solo operaciones de lectura (queries)
//
// =====================================================================================

using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Repositorio para operaciones de acceso a datos de productos
    /// Proporciona métodos de consulta optimizados con EF Core
    /// </summary>
    public interface IProductoRepository
    {
        /// <summary>
        /// Obtiene todos los productos activos del catálogo
        /// </summary>
        /// <returns>Colección de productos activos ordenados por código</returns>
        Task<IEnumerable<Producto>> GetAllProductosAsync();

        /// <summary>
        /// Busca productos por código o nombre utilizando filtro LIKE
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda para código o nombre de producto</param>
        /// <returns>Colección de productos que coinciden con el término de búsqueda</returns>
        /// <remarks>
        /// Búsqueda insensible a mayúsculas/minúsculas
        /// Busca en CodigoProducto y NombreProducto con LIKE %searchTerm%
        /// Solo retorna productos activos
        /// </remarks>
        Task<IEnumerable<Producto>> SearchProductosByFilterAsync(string searchTerm);
    }
}
