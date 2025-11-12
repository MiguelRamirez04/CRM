// =====================================================================================
// INTERFACE REPOSITORY ALMACEN - IAlmacenRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para operaciones de acceso a datos de almacenes.
// Abstrae la base de datos de la lógica de negocio para almacenes.
//
// MÉTODOS PRINCIPALES:
// - GetAllAsync: Obtener todos los almacenes activos
// - GetByIdAsync: Obtener almacén por ID
// - ExistsAsync: Verificar existencia de almacén
//
// =====================================================================================

using back_cabs.CRM.models.legacy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Contrato para operaciones de acceso a datos de almacenes
    /// Esta interfaz abstrae la base de datos de la lógica de negocio
    /// </summary>
    public interface IAlmacenRepository
    {
        // ---------------------------------------------------------------------
        // 📖 QUERIES (Lectura)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todos los almacenes activos del catálogo
        /// </summary>
        /// <returns>Lista de almacenes activos</returns>
        Task<IEnumerable<Almacen>> GetAllAsync();

        /// <summary>
        /// Obtiene un almacén por su ID
        /// </summary>
        /// <param name="id">ID del almacén</param>
        /// <returns>Almacén encontrado o null</returns>
        Task<Almacen?> GetByIdAsync(int id);

        /// <summary>
        /// Verifica si existe un almacén con el ID especificado
        /// </summary>
        /// <param name="id">ID del almacén a verificar</param>
        /// <returns>true si existe, false en caso contrario</returns>
        Task<bool> ExistsAsync(int id);

        // ---------------------------------------------------------------------
        // ✏️ COMMANDS (Escritura) - Futuras implementaciones
        // ---------------------------------------------------------------------

        // Métodos para operaciones de escritura se agregarían aquí cuando sean necesarias
        // - CreateAsync
        // - UpdateAsync
        // - DeleteAsync
    }
}