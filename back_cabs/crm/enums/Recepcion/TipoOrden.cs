// =====================================================================================
// ENUM TIPO ORDEN - TipoOrden.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los tipos de órdenes disponibles en el módulo de Recepción.
//
// CUÁNDO USARLO:
// - Validación de datos de entrada
// - Filtros en consultas
// - Diferenciación entre cotizaciones y asesorías
//
// =====================================================================================

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace back_cabs.CRM.enums
{
    /// <summary>
    /// Tipo de orden de trabajo: cotización o asesoría
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipoOrden
    {
        [Description("Orden de cotización")]
        Cotizacion = 1,

        [Description("Orden de asesoría")]
        Asesoria = 2
    }
}