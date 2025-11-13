// =====================================================================================
// CONCEPTO REPOSITORY INTERFACE - IConceptoRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para el repositorio de conceptos.
// Proporciona métodos para consultar conceptos desde VwConceptosCompletos.
//
// MÉTODOS:
// - GetAllConceptosAsync(): Obtiene todos los conceptos activos
// - SearchConceptosByFilterAsync(searchTerm): Busca por CodigoConcepto o NombreConcepto
//
// =====================================================================================

using back_cabs.CRM.models;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz del repositorio para conceptos
    /// Define operaciones de consulta sobre VwConceptosCompletos
    /// </summary>
    public interface IConceptoRepository
    {
        /// <summary>
        /// Obtiene todos los conceptos activos desde VwConceptosCompletos
        /// </summary>
        /// <returns>Lista de conceptos activos</returns>
        Task<IEnumerable<VwConceptosCompletos>> GetAllConceptosAsync();

        /// <summary>
        /// Busca conceptos por término de búsqueda en CodigoConcepto o NombreConcepto
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <returns>Lista de conceptos que coinciden con el filtro</returns>
        Task<IEnumerable<VwConceptosCompletos>> SearchConceptosByFilterAsync(string searchTerm);
    }
}
