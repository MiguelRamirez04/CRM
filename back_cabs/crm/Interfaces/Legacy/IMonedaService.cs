// =====================================================================================
// INTERFACE SERVICE MONEDA - IMonedaService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para la lógica de negocio de monedas.
// Abstrae la lógica de aplicación de la capa de presentación.
//
// MÉTODOS PRINCIPALES:
// - GetAllMonedasAsync: Obtener todas las monedas activas (mapeadas a DTO)
// - GetMonedaByIdAsync: Obtener moneda por ID (mapeada a DTO)
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Contrato para operaciones de lógica de negocio de monedas
    /// Esta interfaz abstrae la lógica de aplicación de la presentación
    /// </summary>
    public interface IMonedaService
    {
        // ---------------------------------------------------------------------
        // 📖 BUSINESS LOGIC (Lógica de Negocio)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todas las monedas activas del catálogo
        /// </summary>
        /// <returns>Lista de monedas activas mapeadas a DTOs de respuesta</returns>
        Task<IEnumerable<MonedaResponseDto>> GetAllMonedasAsync();

        /// <summary>
        /// Obtiene una moneda por su ID
        /// </summary>
        /// <param name="id">ID de la moneda</param>
        /// <returns>Moneda mapeada a DTO de respuesta o null si no existe</returns>
        Task<MonedaResponseDto?> GetMonedaByIdAsync(int id);

        // ---------------------------------------------------------------------
        // 🔍 BUSINESS VALIDATIONS (Validaciones de Negocio)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Valida si una moneda existe y está activa
        /// </summary>
        /// <param name="id">ID de la moneda a validar</param>
        /// <returns>true si la moneda existe y está activa</returns>
        Task<bool> ValidateMonedaExistsAsync(int id);

        // ---------------------------------------------------------------------
        // ✏️ BUSINESS OPERATIONS (Operaciones de Negocio) - Futuras implementaciones
        // ---------------------------------------------------------------------

        // Métodos para operaciones complejas de negocio se agregarían aquí cuando sean necesarias
        // - ConvertirMonedaAsync
        // - ObtenerTipoCambioAsync
        // - ValidarMonedaParaTransaccionAsync
    }
}