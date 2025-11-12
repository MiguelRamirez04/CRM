// =====================================================================================
// INTERFACE SERVICE ALMACEN - IAlmacenService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para la lógica de negocio de almacenes.
// Abstrae la lógica de aplicación de la capa de presentación.
//
// MÉTODOS PRINCIPALES:
// - GetAllAlmacenesAsync: Obtener todos los almacenes activos (mapeados a DTO)
// - GetAlmacenByIdAsync: Obtener almacén por ID (mapeado a DTO)
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Contrato para operaciones de lógica de negocio de almacenes
    /// Esta interfaz abstrae la lógica de aplicación de la presentación
    /// </summary>
    public interface IAlmacenService
    {
        // ---------------------------------------------------------------------
        // 📖 BUSINESS LOGIC (Lógica de Negocio)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todos los almacenes activos del catálogo
        /// </summary>
        /// <returns>Lista de almacenes activos mapeados a DTOs de respuesta</returns>
        Task<IEnumerable<AlmacenResponseDto>> GetAllAlmacenesAsync();

        /// <summary>
        /// Obtiene un almacén por su ID
        /// </summary>
        /// <param name="id">ID del almacén</param>
        /// <returns>Almacén mapeado a DTO de respuesta o null si no existe</returns>
        Task<AlmacenResponseDto?> GetAlmacenByIdAsync(int id);

        // ---------------------------------------------------------------------
        // 🔍 BUSINESS VALIDATIONS (Validaciones de Negocio)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Valida si un almacén existe y está activo
        /// </summary>
        /// <param name="id">ID del almacén a validar</param>
        /// <returns>true si el almacén existe y está activo</returns>
        Task<bool> ValidateAlmacenExistsAsync(int id);

        // ---------------------------------------------------------------------
        // ✏️ BUSINESS OPERATIONS (Operaciones de Negocio) - Futuras implementaciones
        // ---------------------------------------------------------------------

        // Métodos para operaciones complejas de negocio se agregarían aquí cuando sean necesarias
        // - GetAlmacenesByClasificacionAsync
        // - GetAlmacenesActivosAsync
        // - ValidarAlmacenParaMovimientoAsync
    }
}