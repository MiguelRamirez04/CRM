// =====================================================================================
// ENUM PRIORIDAD - Prioridad.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los niveles de prioridad disponibles para las órdenes de trabajo.
//
// CUÁNDO USARLO:
// - Validación de datos de entrada
// - Ordenamiento de órdenes por prioridad
// - Filtros en consultas
//
// =====================================================================================

using System.ComponentModel;

namespace back_cabs.CRM.enums
{
    /// <summary>
    /// Prioridad de la orden de trabajo (1 = más baja, 5 = más alta)
    /// </summary>
    public enum Prioridad
    {
        [Description("Muy baja")]
        Uno = 1,

        [Description("Baja")]
        Dos = 2,

        [Description("Media")]
        Tres = 3,

        [Description("Alta")]
        Cuatro = 4,

        [Description("Muy alta")]
        Cinco = 5
    }
}