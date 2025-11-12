// =====================================================================================
// INTERFACE SERVICE AGENTE - IAgenteService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para la lógica de negocio de agentes.
// Abstrae la lógica de aplicación de la capa de presentación.
//
// MÉTODOS PRINCIPALES:
// - GetAllAgentesAsync: Obtener todos los agentes activos (mapeados a DTO)
// - GetAgenteByIdAsync: Obtener agente por ID (mapeado a DTO)
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Contrato para operaciones de lógica de negocio de agentes
    /// Esta interfaz abstrae la lógica de aplicación de la presentación
    /// </summary>
    public interface IAgenteService
    {
        // ---------------------------------------------------------------------
        // 📖 BUSINESS LOGIC (Lógica de Negocio)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todos los agentes activos del catálogo
        /// </summary>
        /// <returns>Lista de agentes activos mapeados a DTOs de respuesta</returns>
        Task<IEnumerable<AgenteResponseDto>> GetAllAgentesAsync();

        /// <summary>
        /// Obtiene un agente por su ID
        /// </summary>
        /// <param name="id">ID del agente</param>
        /// <returns>Agente mapeado a DTO de respuesta o null si no existe</returns>
        Task<AgenteResponseDto?> GetAgenteByIdAsync(int id);

        // ---------------------------------------------------------------------
        // 🔍 BUSINESS VALIDATIONS (Validaciones de Negocio)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Valida si un agente existe y está activo
        /// </summary>
        /// <param name="id">ID del agente a validar</param>
        /// <returns>true si el agente existe y está activo</returns>
        Task<bool> ValidateAgenteExistsAsync(int id);

        // ---------------------------------------------------------------------
        // ✏️ BUSINESS OPERATIONS (Operaciones de Negocio) - Futuras implementaciones
        // ---------------------------------------------------------------------

        // Métodos para operaciones complejas de negocio se agregarían aquí cuando sean necesarias
        // - GetAgentesByTipoAsync
        // - GetAgentesActivosAsync
        // - ValidarAgenteParaAsignacionAsync
    }
}