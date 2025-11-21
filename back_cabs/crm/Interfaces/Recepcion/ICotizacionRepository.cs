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
        /// Obtiene cotizaciones filtradas por un campo de estado específico
        /// </summary>
        /// <param name="campo">Campo a filtrar: "cancelado", "afectado", "impreso", "usaCliente"</param>
        /// <param name="valor">Valor del campo: 0 o 1</param>
        Task<IEnumerable<Cotizacion>> GetByEstadoAsync(string campo, int valor);

        /// <summary>
        /// Obtiene cotizaciones por ID de cliente (ClienteProveedorId)
        /// </summary>
        Task<IEnumerable<Cotizacion>> GetByClienteIdAsync(int clienteId);

        /// <summary>
        /// Verifica si existe una cotización con el ID especificado
        /// </summary>
        Task<bool> ExistsAsync(int id);

<<<<<<< HEAD
=======
        /// <summary>
        /// Obtiene cotizaciones creadas en una fecha específica (para generar folios)
        /// </summary>
        Task<IEnumerable<Cotizacion>> GetByFechaCreadoAsync(DateTime fecha);
        /// Valida que todas las llaves foráneas existan en la base de datos
        /// </summary>
        Task<Dictionary<string, bool>> ValidarLlavesForaneasAsync(int documentoDeId, int conceptoDocumentoId, int clienteProveedorId, int agenteId);

>>>>>>> 29afbe45571ab99f1c722a38a504c27ea9e3be5c
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