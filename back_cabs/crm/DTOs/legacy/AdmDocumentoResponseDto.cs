namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO de respuesta simplificado para AdmDocumentos (Cotizaciones/Documentos)
    /// Solo campos esenciales: Serie, Folio, Fecha, RazonSocial, FechaVencimiento, Agente, FechaProntoPago, FechaEntregaRecepcion, Productos
    /// </summary>
    public class AdmDocumentoResponseDto
    {
        public string SerieDocumento { get; set; } = string.Empty;
        public double Folio { get; set; }
        public DateTime Fecha { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public DateTime FechaProntoPago { get; set; }
        public DateTime FechaEntregaRecepcion { get; set; }
        
        // Agente de ventas (nombre completo)
        public string? Agente { get; set; }
        
        // Detalle de movimientos (productos cotizados)
        public List<AdmMovimientoResponseDto> Movimientos { get; set; } = new();
    }
}
