using System;

namespace CRM.DTOs.Response;

/// <summary>
<<<<<<< HEAD
/// DTO para la respuesta de una Cotización.
/// Contiene la información completa de la cotización para ser mostrada al cliente.
=======
/// DTO de respuesta para cotizaciones.
/// Incluye todos los campos de la tabla sales_cotizaciones.
>>>>>>> 29afbe45571ab99f1c722a38a504c27ea9e3be5c
/// </summary>
public class CotizacionResponseDto
{
    // --- Identificadores ---
    public int Id { get; set; }
<<<<<<< HEAD
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
=======
    public int? OrdenId { get; set; }
    public int? IntakeLegacyId { get; set; }
    
    public decimal Subtotal { get; set; }
    public decimal ImpuestosTotal { get; set; }
    
    /// <summary>
    /// Total calculado de BD: Subtotal + ImpuestosTotal (columna PERSISTED)
    /// </summary>
    public decimal Total { get; set; }
    
    public string Estado { get; set; } = "NUEVA";
    public string? Observaciones { get; set; }
    
    public DateTime? ActualizadoEn { get; set; }
    public DateTime CreadoEn { get; set; }
    
    public int? ValidezDias { get; set; }
    
    // ===================================================================
    // CAMPOS DE CAPACITACIÓN
    // ===================================================================
    
    public int? HorasCapacitacion { get; set; }
    public int? PaquetesCapacitacion { get; set; }
    public decimal? CostoCapacitacion { get; set; }
    
    // ===================================================================
    // CAMPOS DE INFORMACIÓN DEL CLIENTE
    // ===================================================================
    
    public string? Cliente { get; set; }
    public string? Rfc { get; set; }
    public string? Folio { get; set; }
    
    // ===================================================================
    // CAMPOS ADICIONALES
    // ===================================================================
    
    public decimal? Descuento { get; set; }
    public string? DescripcionServicio { get; set; }
    
    // ===================================================================
    // CAMPOS DE CONTACTO
    // ===================================================================
    
    /// <summary>
    /// Teléfono de contacto del cliente.
    /// Ejemplo: 6178907616
    /// </summary>
    public long? Telefono { get; set; }
    
    /// <summary>
    /// Correo electrónico de contacto del cliente.
    /// </summary>
    public string? Correo { get; set; }
    
    /// <summary>
    /// Total final considerando descuento: Total - Descuento
    /// Calculado en el servidor, no existe en BD
    /// </summary>
    public decimal TotalFinal => Total - (Descuento ?? 0);
>>>>>>> 29afbe45571ab99f1c722a38a504c27ea9e3be5c
}