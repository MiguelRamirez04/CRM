using back_cabs.CRM.models.Recepcion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back_cabs.CRM.Interfaces.Recepcion
{
    /// <summary>
    /// Contrato para operaciones de acceso a datos de Órdenes de Trabajo.
    /// Esta interfaz abstrae la base de datos de la lógica de negocio.
    /// </summary>
    public interface IOrdenTrabajoRepository
    {
        // ---------------------------------------------------------------------
        // 📖 QUERIES (Lectura)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene una Orden de Trabajo por su ID.
        /// </summary>
        Task<OrdenTrabajo?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todas las Órdenes de Trabajo con opciones de filtrado y paginación.
        /// NOTA: El servicio aplicará el mapeo a DTO.
        /// </summary>
        Task<IEnumerable<OrdenTrabajo>> GetAllFilteredAsync(int? skip, int? take, string? estado);

        /// <summary>
        /// Verifica la existencia de un usuario por ID.
        /// (Necesario para la validación de la FK CreadoPorUserId).
        /// </summary>
        Task<bool> UsuarioExistsAsync(int userId);

        /// <summary>
        /// Verifica si la orden con el ID especificado existe.
        /// </summary>
        Task<bool> OrdenExistsAsync(int id);

        /// <summary>
        /// Obtiene todos los estados de órdenes para estadísticas.
        /// </summary>
        Task<IEnumerable<string>> GetAllEstadosAsync();

        // ---------------------------------------------------------------------
        // COMMANDS (Escritura)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Crea y persiste una nueva Orden de Trabajo.
        /// </summary>
        Task<OrdenTrabajo> CreateAsync(OrdenTrabajo orden);

        /// <summary>
        /// Actualiza una Orden de Trabajo existente y persiste los cambios.
        /// </summary>
        /// <param name="orden">La entidad OrdenTrabajo con los campos actualizados.</param>
        /// <returns>True si la actualización fue exitosa.</returns>
        Task<bool> UpdateAsync(OrdenTrabajo orden);
        
        // ---------------------------------------------------------------------
        // MÉTODOS AUXILIARES (Para búsquedas de clientes dentro del dominio)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Busca clientes Legacy/Completos por nombre o RFC (usa la vista mapeada).
        /// </summary>
        Task<IEnumerable<dynamic>> FindClientesLegacyAsync(string termino, int limite);

        /// <summary>
        /// Agrupa y obtiene la lista de clientes nuevos registrados en órdenes.
        /// </summary>
        Task<IEnumerable<dynamic>> GetClientesNuevosAgrupadosAsync();
    }
}