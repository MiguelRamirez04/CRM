using System;
using System.ComponentModel.DataAnnotations;

namespace CRM.DTOs.Request
{
    /// <summary>
    /// DTO para la creación y actualización de una Cotización.
    /// Contiene los datos que el cliente debe proporcionar.
    /// </summary>
    public class CotizacionRequestDto
    {
        // --- Llaves Foráneas (IDs) ---
        [Required]
        public int DocumentoDeId { get; set; }
        [Required]
        public int ConceptoDocumentoId { get; set; }
        [Required]
        public int ClienteProveedorId { get; set; }
        [Required]
        public int AgenteId { get; set; }
        public int? DocumentoOrigenId { get; set; }

        // --- Datos del Documento ---
        [StringLength(11)]
        public string? SerieDocumento { get; set; }
        public double? Folio { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public DateTime? FechaEntregaRecepcion { get; set; }

        // --- Datos Descriptivos ---
        [StringLength(60)]
        public string? RazonSocial { get; set; }
        [StringLength(20)]
        public string? Rfc { get; set; }
        [StringLength(20)]
        public string? Referencia { get; set; }
        public string? Observaciones { get; set; }

        // --- Banderas y Estados ---
        public int Naturaleza { get; set; }
        public int UsaCliente { get; set; }
        public int Afectado { get; set; }
        public int Impreso { get; set; }
        public int Cancelado { get; set; }

        // --- Importes ---
        public double Neto { get; set; }
        public double Impuesto1 { get; set; }
        public double DescuentoMovimiento { get; set; }
        public double Total { get; set; }
        public double TotalUnidades { get; set; }
    }
}
