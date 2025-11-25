using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace back_cabs.CRM.DTOs.shared
{
    public class EvaluacionResponseDto
    {
        public int Id { get; set; }
        public int ordenId { get; set; }
        public int? EjecucoinId { get; set; }
        public int? CLienteId { get; set; }
        public int EvaluadirId { get; set; }
        public string? Objetivo { get; set; }
        public string? ComentariosGenerales { get; set; }
        public int? ScoreCalidadTotal { get; set; }
        public bool? RequiereSeguimiento { get; set; }
        public string? SeguimientoNotas { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}