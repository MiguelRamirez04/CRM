using back_cabs.CRM.DTOs.Soporte;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using back_cabs.CRM.models.Soporte;

namespace back_cabs.CRM.Interfaces.Soporte
{
    /// <summary>
    /// Contrato de la capa de servicio para todas las operaciones de negocio
    /// relacionadas con Reparaciones y sus Componentes.
    /// </summary>
    public interface IReparacionRepository
    {
        // =====================================================================
        // OPERACIONES DE REPARACIÓN (CRUD)
        // =====================================================================

        /// <summary>
        /// Obtiene una reparación por ID para su MODIFICACIÓN.
        /// La entidad DEVUELTA DEBE ser rastreada por el WriteContext.
        /// </summary>
        Task<Reparacion?> GetReparacionForUpdateAsync(int id);

        /// <summary>
        /// Obtiene todas las reparaciones, con soporte para paginación.
        /// </summary>
        Task<List<ReparacionResponseDto>> ObtenerReparacionesAsync(int? skip = null, int? take = null);

        /// <summary>
        /// Obtiene una reparación específica por ID.
        /// </summary>
        Task<ReparacionResponseDto?> ObtenerReparacionPorIdAsync(int id);

        /// <summary>
        /// Crea una nueva reparación en el sistema.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Si la Orden o el Técnico no existen.</exception>
        Task<ReparacionResponseDto> CrearReparacionAsync(ReparacionCreacionRequestDto request);

        /// <summary>
        /// Actualiza los campos modificables de una reparación y aplica las reglas de negocio.
        /// </summary>
        /// <returns>Tupla con las filas afectadas y el DTO actualizado.</returns>
        /// <exception cref="KeyNotFoundException">Si la reparación no existe.</exception>
        /// <exception cref="ArgumentException">Si el Resultado o TipoEntrega son inválidos.</exception>
        Task<(int FilasAfectadas, ReparacionResponseDto? ReparacionActualizada)> ActualizarReparacionAsync(Reparacion reparacionActualizada);

        /// <summary>
        /// Verifica si una orden de trabajo existe por su ID.
        /// </summary>
        /// <param name="ordenId"></param>
        /// <returns></returns>
        Task<bool> OrdenExisteAsync(int ordenId);
        
        /// <summary>
        /// Verifica si un técnico existe por su ID.
        /// </summary>
        /// <param name="tecnicoId"></param>
        /// <returns></returns>
        Task<bool> TecnicoExisteAsync(int tecnicoId);



        // =====================================================================
        // OPERACIONES DE COMPONENTES (CRUD)
        // =====================================================================

        /// <summary>
        /// Obtiene una lista paginada de todos los componentes de reparación registrados.
        /// </summary>
        Task<List<ReparacionComponenteResponseDto>> ObtenerComponentesReparacionAsync(int? skip = null, int? take = null);

        /// <summary>
        /// Obtiene un componente de reparación por su ID.
        /// </summary>
        Task<ReparacionComponenteResponseDto?> ObtenerComponenteReparacionPorIdAsync(int id);

        /// <summary>
        /// Obtiene un componente de reparación por ID para su MODIFICACIÓN.
        /// La entidad DEVUELTA DEBE ser rastreada por el WriteContext.
        /// </summary>
        Task<ReparacionComponente?> GetComponenteForUpdateAsync(int id);

        /// <summary>
        /// Crea un nuevo componente asociado a una reparación existente.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Si la Reparación no existe.</exception>
        /// <exception cref="ArgumentException">Si el componente es nulo o vacío.</exception>
        Task<ReparacionComponenteResponseDto> CrearComponenteReparacionAsync(ReparacionComponenteRequestDto request);

        /// <summary>
        /// Actualiza los detalles de un componente de reparación.
        /// </summary>
        /// <returns>Tupla con las filas afectadas y el DTO actualizado.</returns>
        /// <exception cref="KeyNotFoundException">Si el componente no existe.</exception>
        Task<(int FilasAfectadas, ReparacionComponenteResponseDto? ReparacionComponenteActualizada)> ActualizarComponenteReparacionAsync( ReparacionComponente Componente);
    }
}