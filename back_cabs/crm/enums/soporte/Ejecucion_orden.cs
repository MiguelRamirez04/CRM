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
// NOTAS IMPORTANTES:
// - Usa JsonConverter para aceptar strings en JSON (no solo números)
// - Compatible con Swagger para documentación automática
//
// =====================================================================================

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace back_cabs.CRM.enums
{
    /// <summary>
    /// Tipo de ejecución de orden de trabajo
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipoEjecucion
    {
        /// <summary>
        /// Ejecución remota (sesión virtual, sin desplazamiento)
        /// </summary>
        [Description("Sesión remota sin desplazamiento físico")]
        REMOTO = 0,

        /// <summary>
        /// Ejecución de campo (visita presencial con vehículo)
        /// </summary>
        [Description("Visita presencial con desplazamiento en vehículo")]
        CAMPO = 1
    }
}