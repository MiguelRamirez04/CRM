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

namespace back_cabs.CRM.enums
{
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

    public enum ResultadoReparacion
    {
        [Description("Irreparable")]
        IRREPARABLE = 1,

        [Description("Reparado")]
        REPARADO = 2,

        [Description("Cotizado")]
        COTIZADO = 3,

        [Description("Devuelto sin Reparar")]
        DEVUELTO_SIN_REPARAR = 4,

        [Description("Sin Reparar")]
        SIN_REPARAR = 5
    }

    public enum TipoEntrega
    {
        [Description("Recogida en Taller")]
        RECOGE_CLIENTE = 1,

        [Description("Envío a Domicilio")]
        DOMICILIO = 2
    }
}