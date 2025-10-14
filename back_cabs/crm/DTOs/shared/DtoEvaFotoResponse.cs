using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace back_cabs.CRM.DTOs.shared
{
    public class EvaluacionFotoResponseDto
    {
        public int Id { get; set; }
        public int DetalleId { get; set; }
        public int DocumentoId { get; set; }
        public string? Tipo { get; set; }
        public string? Descripcion { get; set; }
        public DateTime CreadoEn { get; set; }
    }
} 