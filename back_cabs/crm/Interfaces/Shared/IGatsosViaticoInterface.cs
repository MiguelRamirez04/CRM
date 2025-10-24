using back_cabs.CRM.enums;
using back_cabs.CRM.models.Shared;

namespace back_cabs.CRM.Interfaces
{
    // Esta es la NUEVA interfaz para el Repositorio
    public interface IGastoViaticoRepository
    {
        /// <summary>
        /// Valida si un usuario existe en la base de datos.
        /// </summary>
        Task<bool> UsuarioExistsAsync(int usuarioId);

        /// <summary>
        /// Crea un nuevo viático y sus detalles en una transacción.
        /// </summary>
        Task<GastoViatico> CreateViaticoAsync(GastoViatico viatico);

        /// <summary>
        /// Obtiene un viático por ID, incluyendo sus detalles (para lectura).
        /// </summary>
        Task<GastoViatico?> GetViaticoByIdReadOnlyAsync(int id);

        /// <summary>
        /// Obtiene un viático por ID (para escritura/actualización).
        /// </summary>
        Task<GastoViatico?> GetViaticoByIdForUpdateAsync(int id);

        /// <summary>
        /// Obtiene una lista paginada de viáticos según los filtros.
        /// </summary>
        Task<(List<GastoViatico> Items, int TotalCount)> GetViaticosFilteredAsync(
            int? usuarioId,
            TipoViatico? tipoViatico,
            EstadoGasto? estadoGasto,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int pageNumber,
            int pageSize);
        
        /// <summary>
        /// Guarda los cambios en el contexto de escritura.
        /// </summary>
        Task SaveChangesAsync();
    }
}