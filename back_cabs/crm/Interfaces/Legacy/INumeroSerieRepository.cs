// =====================================================================================
// INTERFAZ REPOSITORIO NÚMERO SERIE - INumeroSerieRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para el repositorio de números de serie.
// Abstrae las operaciones de acceso a datos de la vista VwNumerosSerieCompletos.
//
// MÉTODOS:
// - GetAllNumeroSerieAsync(): Obtiene todos los números de serie activos
// - SearchNumeroSerieByFilterAsync(searchTerm): Busca por número de serie
//
// PATRÓN: Repository Pattern para separar lógica de acceso a datos
//
// =====================================================================================

using back_cabs.CRM.models;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz del repositorio para números de serie
    /// Define operaciones de consulta sobre la vista VwNumerosSerieCompletos
    /// </summary>
    public interface INumeroSerieRepository
    {
        /// <summary>
        /// Obtiene números de serie activos con paginación
        /// </summary>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Cantidad de registros por página</param>
        /// <returns>Tupla con datos paginados y total de registros</returns>
        Task<(List<VwNumerosSerieCompletos> Data, int TotalRecords)> GetNumeroSeriePaginatedAsync(int page, int pageSize);

        /// <summary>
        /// Busca números de serie por término con paginación
        /// </summary>
        /// <param name="searchTerm">Término a buscar</param>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Cantidad de registros por página</param>
        /// <returns>Tupla con datos paginados y total de registros</returns>
        Task<(List<VwNumerosSerieCompletos> Data, int TotalRecords)> SearchNumeroSeriePaginatedAsync(string searchTerm, int page, int pageSize);
    }
}
