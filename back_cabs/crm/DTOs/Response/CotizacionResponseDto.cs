namespace CRM.DTOs.Response;

/// <summary>
/// DTO de respuesta para cotizaciones.
/// Incluye todos los campos de la tabla sales_cotizaciones.
/// </summary>
public class CotizacionResponseDto
{
    public int Id { get; set; }
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
}