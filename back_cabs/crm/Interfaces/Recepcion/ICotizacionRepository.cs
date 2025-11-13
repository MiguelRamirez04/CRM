using back_cabs.CRM.models.Sales;

namespace back_cabs.CRM.Interfaces.Recepcion
{
    /// <summary>
    /// Contrato para operaciones de acceso a datos de Cotizaciones.
    /// Define qué operaciones están disponibles sin especificar cómo se implementan.
    /// </summary>
    public interface ICotizacionRepository
    {
        // 📖 OPERACIONES DE LECTURA
        /// <summary>
        /// Obtiene todas las cotizaciones ordenadas por fecha de creación descendente
        /// </summary>
        Task<IEnumerable<Cotizacion>> GetAllAsync();

        /// <summary>
        /// Obtiene una cotización por su ID
        /// </summary>
        Task<Cotizacion?> GetByIdAsync(int id);

        // /// <summary>
        // /// Obtiene cotizaciones por OrdenId
        // /// </summary>
        // Task<IEnumerable<Cotizacion>> GetOrdenServicioAsync (int ordenId);

        /// <summary>
        /// Obtiene cotizaciones por estado
        /// </summary>
        Task<IEnumerable<Cotizacion>> GetByEstadoAsync(string estado);

        /// <summary>
        /// Obtiene cotizaciones por cliente
        /// </summary>
        Task<IEnumerable<Cotizacion>> GetByClienteAsync(string cliente);

        /// <summary>
        /// Verifica si existe una cotización con el ID especificado
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Obtiene cotizaciones creadas en una fecha específica (para generar folios)
        /// </summary>
        Task<IEnumerable<Cotizacion>> GetByFechaCreadoAsync(DateTime fecha);

        // ✏️ OPERACIONES DE ESCRITURA
        /// <summary>
        /// Crea una nueva cotización
        /// </summary>
        Task<Cotizacion> CreateAsync(Cotizacion cotizacion);

        /// <summary>
        /// Actualiza una cotización existente
        /// </summary>
        Task<Cotizacion> UpdateAsync(Cotizacion cotizacion);

        /// <summary>
        /// Elimina una cotización por su ID
        /// </summary>
        Task<bool> DeleteAsync(int id);
    }
}