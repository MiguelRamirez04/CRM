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
<<<<<<< HEAD
        COTIZADO = 3,
=======
        COTIZAR = 3,
>>>>>>> 26ed7eef6405f23b5f35e858f5e4a208e4eb26c6

        [Description("Devuelto sin Reparar")]
        DEVUELTO_SIN_REPARAR = 4,

<<<<<<< HEAD
        [Description("Sin Reparar")]
=======
        [Description("Sin_Reparar")]
>>>>>>> 26ed7eef6405f23b5f35e858f5e4a208e4eb26c6
        SIN_REPARAR = 5
    }

    public enum TipoEntrega
    {
<<<<<<< HEAD
        [Description("Recogida en Taller")]
        RECOGE_CLIENTE = 1,

        [Description("Envío a Domicilio")]
=======
        [Description("Recoge_Cliente")]
        RECOGE_CLIENTE = 1,

        [Description("Domicilio")]
>>>>>>> 26ed7eef6405f23b5f35e858f5e4a208e4eb26c6
        DOMICILIO = 2
    }
}