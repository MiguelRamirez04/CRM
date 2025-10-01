using System;

namespace back_cabs.CRM.models.Recepcion
{
    public class OrdenTrabajo
    {
        public int Id { get; set; }
        public string? Notas { get; set; }
        public DateTime? CitaProgramadaInicio { get; set; }
        public DateTime? CitaProgramadaFin { get; set; }
        public string? Modalidad { get; set; }
        public string? TipoOrden { get; set; }
        public int? ClienteRefId { get; set; }
        public string? Cotizaciones { get; set; }
        public int? LegacyClientId { get; set; }
        public int? Prioridad { get; set; }
        public bool? Estado { get; set; }
        public string? UbicacionText { get; set; }
        public string? EstadoFacturado { get; set; }
        public bool? RequiereFactura { get; set; }
        public int? FacturaFolio { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? ActualizadoEn { get; set; }
        public int? CostoReal { get; set; }
        public int? CostoEstimado { get; set; }

        // Propiedad de navegación para Entity Framework
        public virtual Cliente? Cliente { get; set; }
    }
}
