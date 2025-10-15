using System;

namespace back_cabs.CRM.DTOs.shared
{
    /// <summary>
    /// DTO para devolver información de una foto de evaluación.
    /// </summary>
    public class EvaluacionFotoResponseDto
    {
        public int Id { get; set; }
        public int DetalleId { get; set; }
        public int DocumentoId { get; set; }
        public string? Tipo { get; set; }
        public string? Descripcion { get; set; }
        public DateTime CreadoEn { get; set; }
        
        // Información del archivo
        public string NombreArchivo { get; set; } = string.Empty;
        public string? MimeType { get; set; }
        public long? TamanoBytes { get; set; }
        public string UrlDescarga { get; set; } = string.Empty;
    }
} 