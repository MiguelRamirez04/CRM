// =====================================================================================
// INTERFACE REPOSITORY AGENTE - IAgenteRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para operaciones de acceso a datos de agentes.
// Abstrae la base de datos de la lógica de negocio para agentes.
//
// MÉTODOS PRINCIPALES:
// - GetAllAsync: Obtener todos los agentes activos
// - GetByIdAsync: Obtener agente por ID
// - ExistsAsync: Verificar existencia de agente
//
// =====================================================================================

using back_cabs.CRM.models.legacy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Contrato para operaciones de acceso a datos de agentes
    /// Esta interfaz abstrae la base de datos de la lógica de negocio
    /// </summary>
    public interface IAgenteRepository
    {
        // ---------------------------------------------------------------------
        // 📖 QUERIES (Lectura)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todos los agentes activos del catálogo
        /// </summary>
        /// <returns>Lista de agentes activos</returns>
        Task<IEnumerable<Agente>> GetAllAsync();

        /// <summary>
        /// Obtiene un agente por su ID
        /// </summary>
        /// <param name="id">ID del agente</param>
        /// <returns>Agente encontrado o null</returns>
        Task<Agente?> GetByIdAsync(int id);

        /// <summary>
        /// Verifica si existe un agente con el ID especificado
        /// </summary>
        /// <param name="id">ID del agente a verificar</param>
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