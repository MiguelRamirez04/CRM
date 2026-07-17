// Ruta sugerida: back_cabs.CRM.repositories
namespace back_cabs.CRM.Interfaces
{
    /// <summary>
    /// Define el contrato para las operaciones de validación y depuración 
    /// relacionadas con clientes legacy.
    /// </summary>
    public interface IClientesLegacyValidationRepository
    {
        /// <summary>
        /// Intenta validar la existencia de un cliente legacy utilizando múltiples estrategias
        /// de verificación.
        /// </summary>
        /// <param name="clienteId">El ID a validar (puede ser ClienteId o LegacyClientId).</param>
        /// <returns>Tarea que resuelve a 'true' si el cliente es validado por alguna estrategia, 'false' en caso contrario.</returns>
        Task<bool> ValidarClienteLegacyUsingMultipleStrategiesAsync(int? clienteId);

        /// <summary>
        /// Obtiene información detallada sobre la estructura de la fuente de datos (VwClientesCompletos)
        /// para fines de depuración.
        /// </summary>
        /// <returns>Tarea que resuelve a un string con la información de la estructura o un mensaje de error.</returns>
        Task<string> ObtenerInformacionEstructuraAsync();
    }
}