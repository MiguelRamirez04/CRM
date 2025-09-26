// =====================================================================================
// ENUM TIPO TRANSMISIÓN - TipoTransmision.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los tipos de transmisión de vehículos que puede manejar un usuario.
// Usado para asignar vehículos de empresa según las habilidades del conductor.
//
// CUÁNDO USARLO:
// - Registro de usuarios con licencia de conducir
// - Asignación de vehículos para soportes presenciales
// - Validación de capacidades de conducción
// - Filtros para disponibilidad de vehículos
//
// CÓMO USARLO:
// if (usuario.TransmisionHabilitada.Contains(TipoTransmision.Manual.ToString()))
// {
//     // Puede usar vehículos manuales
// }
//
// =====================================================================================

using System.ComponentModel;

namespace back_cabs.CRM.enums
{
    /// <summary>
    /// Tipos de transmisión de vehículos que puede manejar un usuario
    /// </summary>
    public enum TipoTransmision
    {
        /// <summary>
        /// No puede manejar ningún tipo de vehículo
        /// </summary>
        [Description("Sin habilitación para conducir")]
        Ninguna = 0,

        /// <summary>
        /// Solo puede manejar vehículos con transmisión automática
        /// </summary>
        [Description("Transmisión automática únicamente")]
        Automatico = 1,

        /// <summary>
        /// Solo puede manejar vehículos con transmisión manual
        /// </summary>
        [Description("Transmisión manual únicamente")]
        Manual = 2,

        /// <summary>
        /// Puede manejar tanto automática como manual
        /// </summary>
        [Description("Ambas transmisiones (automática y manual)")]
        Ambas = 3
    }

    /// <summary>
    /// Extensiones para el enum TipoTransmision
    /// </summary>
    public static class TipoTransmisionExtensions
    {
        /// <summary>
        /// Obtiene la descripción del tipo de transmisión
        /// </summary>
        public static string ObtenerDescripcion(this TipoTransmision tipo)
        {
            var field = tipo.GetType().GetField(tipo.ToString());
            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute));
            return attribute?.Description ?? tipo.ToString();
        }

        /// <summary>
        /// Verifica si puede manejar vehículos automáticos
        /// </summary>
        public static bool PuedeManearAutomatico(this TipoTransmision tipo)
        {
            return tipo == TipoTransmision.Automatico || tipo == TipoTransmision.Ambas;
        }

        /// <summary>
        /// Verifica si puede manejar vehículos manuales
        /// </summary>
        public static bool PuedeManearManual(this TipoTransmision tipo)
        {
            return tipo == TipoTransmision.Manual || tipo == TipoTransmision.Ambas;
        }

        /// <summary>
        /// Obtiene los valores válidos como lista de strings
        /// </summary>
        public static List<string> ObtenerValoresValidos()
        {
            return Enum.GetValues<TipoTransmision>()
                      .Select(t => t.ToString())
                      .ToList();
        }
    }
}