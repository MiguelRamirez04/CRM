using System;

namespace CRM.DTOs.Response;

/// <summary>
/// DTO para la respuesta de una Cotización.
/// Contiene la información completa de la cotización para ser mostrada al cliente.
/// </summary>
public class CotizacionResponseDto
{
    // --- Identificadores ---
    public int Id { get; set; }
    // public int DocumentoDeId { get; set; }
    // public int ConceptoDocumentoId { get; set; }
    // public int ClienteProveedorId { get; set; }
    // public int AgenteId { get; set; }
    // public int? DocumentoOrigenId { get; set; }

    // --- Datos del Documento ---
    public string? SerieDocumento { get; set; }
    public double Folio { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public DateTime? FechaEntregaRecepcion { get; set; }

    // --- Datos Descriptivos ---
    public string? RazonSocial { get; set; }
    public string? Rfc { get; set; }
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }

    // --- Banderas y Estados ---
    public int Naturaleza { get; set; }
    public int UsaCliente { get; set; }
    public int Afectado { get; set; }
    public int Impreso { get; set; }
    public int Cancelado { get; set; }

    // --- Importes y Totales ---
    public double Neto { get; set; }
    public double Impuesto1 { get; set; }
    public double DescuentoMovimiento { get; set; }
    public double Total { get; set; }
    public double Pendiente { get; set; }
    public double TotalUnidades { get; set; }
}