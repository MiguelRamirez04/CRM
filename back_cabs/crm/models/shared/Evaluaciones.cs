using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Shared
{
    [Table("evaluaciones")]
    public class Evaluacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("orden_id")]
        public int OrdenId { get; set; }

        [Column("ejecucion_id")]
        public int? EjecucionId { get; set; }

        [Column("cliente_id")]
        public int? ClienteId { get; set; }

        [Required]
        [Column("evaluador_id")]
        public int EvaluadorId { get; set; }

        [Column("objetivo")]
        [StringLength(200)]
        public string? Objetivo { get; set; }

        [Column("comentarios_generales")]
        public string? ComentariosGenerales { get; set; }

        [Column("score_calidad_total")]
        public int? ScoreCalidadTotal { get; set; }

        [Required]
        [Column("requiere_seguimiento")]
        public bool RequiereSeguimiento { get; set; } = false;

        [Column("seguimiento_notas")]
        public string? SeguimientoNotas { get; set; }

        [Required]
        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }
}