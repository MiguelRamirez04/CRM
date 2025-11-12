// =====================================================================================
// INTERFACE DOCUMENTO MODELO REPOSITORY - IDocumentoModeloRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para operaciones de acceso a datos de documentos modelo.
// Sigue el patrón Repository para abstraer la lógica de persistencia.
//
// MÉTODOS:
// - GetAllDocumentoModelosAsync: Obtiene todos los documentos modelo activos
// - SearchDocumentoModelosByFilterAsync: Busca documentos modelo por descripción
//
// USO:
// - Implementado por DocumentoModeloRepository
// - Inyectado en DocumentoModeloService
// - Solo operaciones de lectura (queries)
//
// =====================================================================================

using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Repositorio para operaciones de acceso a datos de documentos modelo
    /// Proporciona métodos de consulta optimizados con EF Core
    /// </summary>
    public interface IDocumentoModeloRepository
    {
        /// <summary>
        /// Obtiene todos los documentos modelo activos del catálogo
        /// </summary>
        /// <returns>Colección de documentos modelo activos ordenados por descripción</returns>
        Task<IEnumerable<DocumentoModelo>> GetAllDocumentoModelosAsync();

        /// <summary>
        /// Busca documentos modelo por descripción utilizando filtro LIKE
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda para filtrar por descripción</param>
        /// <returns>Colección de documentos modelo que coinciden con el término de búsqueda</returns>
        /// <remarks>
        /// Búsqueda insensible a mayúsculas/minúsculas
        /// Busca en Descripcion con LIKE %searchTerm%
        /// Solo retorna documentos modelo activos
        /// </remarks>
        Task<IEnumerable<DocumentoModelo>> SearchDocumentoModelosByFilterAsync(string searchTerm);
    }
}
