using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.shared
{
    public class DtoEvaDetallesResponse
    {
        public int Id { get; set; }
        public int EvaluacionId { get; set; }
        public string? Fase { get; set; }
        public string? Descripcion { get; set; }
        public string? Sugerencias { get; set; }
        public int? ScoreFase { get; set; }
        public string? EvidenciasNota { get; set; }
        public DateTime CreadoEn { get; set; }
        public string Lugar { get; set; } = string.Empty;
    }
}   