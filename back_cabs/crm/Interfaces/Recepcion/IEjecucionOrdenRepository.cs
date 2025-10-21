using back_cabs.CRM.enums;
using back_cabs.CRM.models.Soporte;

namespace back_cabs.CRM.Interfaces.Recepcion
{
    /// <summary>
    /// Contrato para operaciones de acceso a datos de Ejecuciones de Orden.
    /// Define qué operaciones están disponibles sin especificar cómo se implementan.
    /// </summary>
    public interface IEjecucionOrdenRepository
    {
        // 📖 OPERACIONES DE LECTURA

        /// <summary>
        /// Obtiene todas las ejecuciones con filtros opcionales
        /// </summary>
        Task<IEnumerable<EjecucionOrden>> GetAllAsync(
            int? ordenId = null,
            int? tecnicoId = null,
            TipoEjecucion? tipo = null,
            DateTime? desde = null,
            DateTime? hasta = null);

        /// <summary>
        /// Obtiene una ejecución por su ID con todas las relaciones
        /// </summary>
        Task<EjecucionOrden?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene ejecuciones por orden de trabajo
        /// </summary>
        Task<IEnumerable<EjecucionOrden>> GetByOrdenIdAsync(int ordenId);

        /// <summary>
        /// Obtiene ejecuciones por técnico asignado
        /// </summary>
        Task<IEnumerable<EjecucionOrden>> GetByTecnicoIdAsync(int tecnicoId);

        /// <summary>
        /// Obtiene ejecuciones activas (sin HrFin)
        /// </summary>
        Task<IEnumerable<EjecucionOrden>> GetEjecucionesActivasAsync();

        /// <summary>
        /// Verifica si existe una ejecución con el ID especificado
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Verifica si un técnico tiene ejecuciones activas
        /// </summary>
        Task<bool> TecnicoTieneEjecucionesActivasAsync(int tecnicoId);

        /// <summary>
        /// Verifica si una orden tiene ejecuciones activas
        /// </summary>
        Task<bool> OrdenTieneEjecucionesActivasAsync(int ordenId);

        // ✏️ OPERACIONES DE ESCRITURA

        /// <summary>
        /// Crea una nueva ejecución de orden
        /// </summary>
        Task<EjecucionOrden> CreateAsync(EjecucionOrden ejecucion);

        /// <summary>
        /// Actualiza una ejecución existente
        /// </summary>
        Task<EjecucionOrden> UpdateAsync(EjecucionOrden ejecucion);

        /// <summary>
        /// Elimina una ejecución por ID
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Delega una ejecución a otro técnico
        /// </summary>
        Task<EjecucionOrden> DelegateAsync(int ejecucionId, int nuevoTecnicoId, int usuarioActualId);
    }
}