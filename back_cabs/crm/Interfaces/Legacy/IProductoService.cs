// =====================================================================================
// INTERFACE PRODUCTO SERVICE - IProductoService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para la lógica de negocio de productos.
// Incluye integración con cache Redis para optimización de performance.
//
// MÉTODOS:
// - GetAllProductosAsync: Obtiene todos los productos (con cache)
// - SearchProductosAsync: Busca productos por término (con cache)
//
// CACHE STRATEGY:
// - Cache-Aside Pattern (lazy loading)
// - TTL: 30 min (GetAll), 60 min (Search)
// - Keys: productos:all, productos:search:{term}
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Servicio para lógica de negocio de productos
    /// Implementa cache Redis para optimización de performance
    /// </summary>
    public interface IProductoService
    {
        /// <summary>
        /// Obtiene todos los productos activos del catálogo
        /// Implementa cache Redis con TTL de 30 minutos
        /// </summary>
        /// <returns>Colección de DTOs de productos activos</returns>
        /// <remarks>
        /// Cache Key: "productos:all"
        /// Cache TTL: 30 minutos
        /// Cache Strategy: Cache-Aside (lazy loading)
        /// Fallback: Si Redis falla, consulta BD directamente
        /// </remarks>
        Task<IEnumerable<ProductoResponseDto>> GetAllProductosAsync();

        /// <summary>
        /// Busca productos por código o nombre
        /// Implementa cache Redis con TTL de 60 minutos
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda para filtrar productos</param>
        /// <returns>Colección de DTOs de productos que coinciden con el término</returns>
        /// <remarks>
        /// Cache Key: "productos:search:{searchTerm.ToLower()}"
        /// Cache TTL: 60 minutos
        /// Búsqueda insensible a mayúsculas/minúsculas
        /// Busca en CodigoProducto y NombreProducto
        /// Retorna array vacío si no hay coincidencias
        /// </remarks>
        Task<IEnumerable<ProductoResponseDto>> SearchProductosAsync(string searchTerm);
    }
}
