using System.ComponentModel;

namespace back_cabs.CRM.enums
{
    /// <summary>
    /// Modalidad de la orden de trabajo: presencial o virtual
    /// </summary>
    public enum Modalidad
    {
        [Description("Asesoria presencial")]
        Presencial = 1,

        [Description("Asesoria virtual")]
        Virtual = 2
    }
}