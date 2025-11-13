// =====================================================================================
// CONCEPTO SERVICE INTERFACE - IConceptoService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para el servicio de conceptos.
// Proporciona métodos de negocio con cache Redis integrado.
//
// MÉTODOS:
// - GetAllConceptosAsync(): Obtiene todos los conceptos activos con cache
// - SearchConceptosAsync(searchTerm): Busca por CodigoConcepto/NombreConcepto con cache
//
// CACHE STRATEGY:
// - GetAll: TTL 30 minutos (datos estables)
// - Search: TTL 60 minutos (búsquedas repetitivas)
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz del servicio para conceptos
    /// Define operaciones de negocio con cache Redis integrado
    /// </summary>
    public interface IConceptoService
    {
        /// <summary>
        /// Obtiene todos los conceptos activos con cache Redis
        /// </summary>
        /// <returns>Lista de conceptos activos mapeados a DTO</returns>
        Task<IEnumerable<ConceptoResponseDto>> GetAllConceptosAsync();

        /// <summary>
        /// Busca conceptos por término con cache Redis
        /// Busca en CodigoConcepto y NombreConcepto
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <returns>Lista de conceptos que coinciden con el criterio</returns>
        Task<IEnumerable<ConceptoResponseDto>> SearchConceptosAsync(string searchTerm);
    }
}
