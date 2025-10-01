using System.ComponentModel;

namespace back_cabs.CRM.enums
{
    /// <summary>
    /// Tipo de orden de trabajo: cotización o asesoría
    /// </summary>
    public enum TipoOrden
    {
        [Description("Orden de cotización")]
        Cotizacion = 1,

        [Description("Orden de asesoría")]
        Asesoria = 2
    }
}