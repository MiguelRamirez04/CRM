using back_cabs.CRM.models.Shared;

namespace back_cabs.CRM.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de gastos de viáticos
    /// </summary>
    public interface IGastoViaticoRepository
    {
        /// <summary>
        /// Crea un nuevo viático en la base de datos
        /// </summary>
        Task<GastoViatico> CreateViaticoAsync(GastoViatico viatico);

        /// <summary>
        /// Obtiene un viático por ID (solo lectura)
        /// </summary>
        Task<GastoViatico?> GetViaticoByIdReadOnlyAsync(int id);

        /// <summary>
        /// Obtiene un viático por ID (para actualización)
        /// </summary>
        Task<GastoViatico?> GetViaticoByIdForUpdateAsync(int id);

        /// <summary>
        /// Obtiene un viático por ID incluyendo el vehículo relacionado (si existe)
        /// </summary>
        Task<GastoViatico?> GetViaticoConVehiculoAsync(int id);

        /// <summary>
        /// Obtiene una lista paginada de viáticos según los filtros
        /// </summary>
        Task<(List<GastoViatico> Items, int TotalCount)> GetViaticosFilteredAsync(
            int? ordenId,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int pageNumber,
            int pageSize);

        /// <summary>
        /// Guarda los cambios en el contexto de escritura
        /// </summary>
        Task SaveChangesAsync();
    }
}