using System;
using System.Collections.Generic;

namespace back_cabs.CRM.models.Recepcion
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? EstadoFiscal { get; set; }
        public string? RFC { get; set; }

        // Propiedad de navegación para Entity Framework
        public virtual ICollection<OrdenTrabajo> OrdenesTrabajo { get; set; } = new List<OrdenTrabajo>();
    }
}
