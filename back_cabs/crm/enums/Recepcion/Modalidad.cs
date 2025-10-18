// =====================================================================================
// ENUM MODALIDAD - Modalidad.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define las modalidades disponibles para las órdenes de trabajo.
//
// CUÁNDO USARLO:
// - Validación de datos de entrada
// - Filtros en consultas
// - Documentación Swagger
//
// =====================================================================================

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace back_cabs.CRM.enums
{
    /// <summary>
    /// Modalidad de la orden de trabajo: presencial o virtual
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Modalidad
    {
        [Description("Asesoría presencial")]
        Presencial = 1,

        [Description("Asesoría virtual")]
        Virtual = 2
    }
}