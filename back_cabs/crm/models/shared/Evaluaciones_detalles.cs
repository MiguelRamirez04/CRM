using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Shared
{
    [Table("evaluaciones_detalles")]
    public class EvaluacionDetalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("evaluacion_id")]
        public int EvaluacionId { get; set; }

        [Required]
        [Column("fase")]
        [StringLength(10)]
        public string Fase { get; set; } = string.Empty;

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("sugerencias")]
        public string? Sugerencias { get; set; }

        [Column("score_fase")]
        public int? ScoreFase { get; set; }

        [Column("evidencia_nota")]
        public string? EvidenciaNota { get; set; }

        [Required]
        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("lugar")]
        [StringLength(100)]
        public string Lugar { get; set; } = string.Empty;
    }
}