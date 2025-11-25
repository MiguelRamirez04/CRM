// =====================================================================================
// ENUM TIPO EJECUCION - Ejecucion_orden.cs
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
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipoEjecucion
    {
        REMOTO,
        CAMPO
    }
}