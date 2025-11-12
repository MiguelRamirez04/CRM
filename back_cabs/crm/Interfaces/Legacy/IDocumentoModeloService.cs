// =====================================================================================
// INTERFACE DOCUMENTO MODELO SERVICE - IDocumentoModeloService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para la lógica de negocio de documentos modelo.
// Incluye integración con cache Redis para optimización de performance.
//
// MÉTODOS:
// - GetAllDocumentoModelosAsync: Obtiene todos los documentos modelo (con cache)
// - SearchDocumentoModelosAsync: Busca documentos modelo por término (con cache)
//
// CACHE STRATEGY:
// - Cache-Aside Pattern (lazy loading)
// - TTL: 30 min (GetAll), 60 min (Search)
// - Keys: documentos_modelo:all, documentos_modelo:search:{term}
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Servicio para lógica de negocio de documentos modelo
    /// Implementa cache Redis para optimización de performance
    /// </summary>
    public interface IDocumentoModeloService
    {
        /// <summary>
        /// Obtiene todos los documentos modelo activos del catálogo
        /// Implementa cache Redis con TTL de 30 minutos
        /// </summary>
        /// <returns>Colección de DTOs de documentos modelo activos</returns>
        /// <remarks>
        /// Cache Key: "documentos_modelo:all"
        /// Cache TTL: 30 minutos
        /// Cache Strategy: Cache-Aside (lazy loading)
        /// Fallback: Si Redis falla, consulta BD directamente
        /// </remarks>
        Task<IEnumerable<DocumentoModeloResponseDto>> GetAllDocumentoModelosAsync();

        /// <summary>
        /// Busca documentos modelo por descripción
        /// Implementa cache Redis con TTL de 60 minutos
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda para filtrar documentos modelo</param>
        /// <returns>Colección de DTOs de documentos modelo que coinciden con el término</returns>
        /// <remarks>
        /// Cache Key: "documentos_modelo:search:{searchTerm.ToLower()}"
        /// Cache TTL: 60 minutos
        /// Búsqueda insensible a mayúsculas/minúsculas
        /// Busca en Descripcion con LIKE %searchTerm%
        /// Retorna array vacío si no hay coincidencias
        /// </remarks>
        Task<IEnumerable<DocumentoModeloResponseDto>> SearchDocumentoModelosAsync(string searchTerm);
    }
}
