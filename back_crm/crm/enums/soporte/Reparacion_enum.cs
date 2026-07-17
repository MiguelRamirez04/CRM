/// enum fotos reparacion
/// enum de resultado reparacion
/// 
// =====================================================================================
// ENUMS ETAPA, RESULTADO Y TIPO ENTREGA  - Reparacion_enum.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define las etapas, resultados y tipos de entrega posibles para una reparación.
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
    public enum EtapaReparacion
    {
        [Description("Recibido")]
        RECIBIDO = 1,

        [Description("En Proceso")]
        PROCESO = 2,

        [Description("Entregado")]
        ENTREGADO = 3,

        [Description("Otro")]
        OTRO = 4
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ResultadoReparacion
    {
        [Description("Irreparable")]
        IRREPARABLE = 1,

        [Description("Reparado")]
        REPARADO = 2,

        [Description("Cotizado")]
        COTIZAR = 3,

        [Description("Devuelto sin Reparar")]
        DEVUELTO_SIN_REPARAR = 4,

        [Description("Sin_Reparar")]
        SIN_REPARAR = 5
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipoEntrega
    {
        [Description("Recoge_Cliente")]
        RECOGE_CLIENTE = 1,

        [Description("Domicilio")]
        DOMICILIO = 2
    }
}