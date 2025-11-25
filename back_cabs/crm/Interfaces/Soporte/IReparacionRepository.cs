using back_cabs.CRM.models.Soporte; // Usar Entidades
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back_cabs.CRM.Interfaces.Soporte
{
    /// <summary>
    /// Contrato para las operaciones de ACCESO A DATOS para Reparaciones y Componentes.
    /// Abstrae la tecnología de persistencia (EF Core).
    /// </summary>
    public interface IReparacionRepository
    {
        // =====================================================================
        // OPERACIONES DE REPARACIÓN (CRUD)
        // =====================================================================

        /// <summary>
        /// Obtiene una reparación por ID CON TRACKING para su modificación.
        /// </summary>
        Task<Reparacion?> GetReparacionForUpdateAsync(int id);

        /// <summary>
        /// Obtiene todas las reparaciones SIN TRACKING, con paginación.
        /// </summary>
        Task<IEnumerable<Reparacion>> ObtenerReparacionesAsync(int? skip = null, int? take = null);

        /// <summary>
        /// Obtiene una reparación específica por ID SIN TRACKING (solo lectura).
        /// </summary>
        Task<Reparacion?> ObtenerReparacionPorIdAsync(int id);

        /// <summary>
<<<<<<< HEAD
        /// Obtiene los componentes específicos por ID de reparacion SIN TRACKING (solo lectura).
        /// </summary>
        Task<IEnumerable<ReparacionComponente>> ObtenerComponentePorIdReparacionAsync(int repID);


        /// <summary>
=======
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
        /// Persiste una nueva entidad Reparacion.
        /// </summary>
        /// <returns>La entidad Reparacion creada (con su ID).</returns>
        Task<Reparacion> CrearReparacionAsync(Reparacion reparacion); // <-- Recibe y devuelve ENTIDAD

        /// <summary>
        /// Persiste los cambios de una entidad Reparacion existente.
        /// </summary>
        /// <returns>Tupla con filas afectadas y la entidad actualizada (opcional).</returns>
        Task<(int FilasAfectadas, Reparacion? ReparacionActualizada)> ActualizarReparacionAsync(Reparacion reparacionActualizada); // <-- Recibe y devuelve ENTIDAD

        /// <summary>
<<<<<<< HEAD
=======
        /// Verifica si una orden de trabajo existe.
        /// </summary>
        Task<bool> OrdenExisteAsync(int ordenId);

        /// <summary>
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
        /// Verifica si un técnico existe.
        /// </summary>
        Task<bool> TecnicoExisteAsync(int tecnicoId);
        
        /// <summary>
        /// Verifica si una reparación existe.
        /// </summary>
        Task<bool> ReparacionExisteAsync(int reparacionId);


        // =====================================================================
        // OPERACIONES DE COMPONENTES (CRUD)
        // =====================================================================

        /// <summary>
        /// Obtiene una lista paginada de componentes SIN TRACKING.
        /// </summary>
        Task<IEnumerable<ReparacionComponente>> ObtenerComponentesReparacionAsync(int? skip = null, int? take = null); // <-- Devuelve ENTIDADES

        /// <summary>
        /// Obtiene un componente por ID SIN TRACKING (solo lectura).
        /// </summary>
        Task<ReparacionComponente?> ObtenerComponenteReparacionPorIdAsync(int id); // <-- Devuelve ENTIDAD

        /// <summary>
        /// Obtiene un componente por ID CON TRACKING para su modificación.
        /// </summary>
        Task<ReparacionComponente?> GetComponenteForUpdateAsync(int id); // <-- Devuelve ENTIDAD

        /// <summary>
        /// Persiste un nuevo componente de reparación.
        /// </summary>
        /// <returns>La entidad Componente creada (con su ID).</returns>
        Task<ReparacionComponente> CrearComponenteReparacionAsync(ReparacionComponente componente); // <-- Recibe y devuelve ENTIDAD

        /// <summary>
        /// Persiste los cambios de un componente existente.
        /// </summary>
        /// <returns>Tupla con filas afectadas y la entidad actualizada.</returns>
<<<<<<< HEAD
        Task<(int FilasAfectadas, ReparacionComponente? ComponenteActualizado)> ActualizarComponenteReparacionAsync(ReparacionComponente componente); // <-- Recibe y devuelve ENTIDAD
    }
=======
        Task<(int FilasAfectadas, ReparacionComponente? ComponenteActualizado)> ActualizarComponenteReparacionAsync(ReparacionComponente componente); // <-- Recibe y devuelve ENTIDAD
    }
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
}