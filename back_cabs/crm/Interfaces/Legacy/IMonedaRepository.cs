// =====================================================================================
// INTERFACE REPOSITORY MONEDA - IMonedaRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para operaciones de acceso a datos de monedas.
// Abstrae la base de datos de la lógica de negocio para monedas.
//
// MÉTODOS PRINCIPALES:
// - GetAllAsync: Obtener todas las monedas activas
// - GetByIdAsync: Obtener moneda por ID
// - ExistsAsync: Verificar existencia de moneda
//
// =====================================================================================

using back_cabs.CRM.models.legacy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Contrato para operaciones de acceso a datos de monedas
    /// Esta interfaz abstrae la base de datos de la lógica de negocio
    /// </summary>
    public interface IMonedaRepository
    {
        // ---------------------------------------------------------------------
        // 📖 QUERIES (Lectura)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todas las monedas activas del catálogo
        /// </summary>
        /// <returns>Lista de monedas activas</returns>
        Task<IEnumerable<Moneda>> GetAllAsync();

        /// <summary>
        /// Obtiene una moneda por su ID
        /// </summary>
        /// <param name="id">ID de la moneda</param>
        /// <returns>Moneda encontrada o null</returns>
        Task<Moneda?> GetByIdAsync(int id);

        /// <summary>
        /// Verifica si existe una moneda con el ID especificado
        /// </summary>
        /// <param name="id">ID de la moneda a verificar</param>
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