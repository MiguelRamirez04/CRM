using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Shared
{
    /// <summary>
    /// Entidad que representa el detalle de una evaluación por fase
    /// </summary>
    [Table("evaluaciones_detalles")]
    public class EvaluacionDetalle
    {
        /// <summary>
        /// Identificador único del detalle (autoincremental)
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID de la evaluación padre
        /// </summary>
        [Required]
        [Column("evaluacion_id")]
        public int EvaluacionId { get; set; }

        /// <summary>
        /// Fase del proceso evaluada
        /// </summary>
        [Required]
        [Column("fase")]
        [StringLength(10)]
        public string Fase { get; set; } = string.Empty;
        //antes o despues

        /// <summary>
        /// Descripción del detalle de evaluación
        /// </summary>
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Sugerencias de mejora
        /// </summary>
        [Column("sugerencias")]
        public string? Sugerencias { get; set; }

        /// <summary>
        /// Score de la fase específica
        /// </summary>
        [Column("score_fase")]
        public int? ScoreFase { get; set; }

        /// <summary>
        /// Nota sobre la evidencia
        /// </summary>
        [Column("evidencia_nota")]
        public string? EvidenciaNota { get; set; }

        /// <summary>
        /// Fecha y hora de creación
        /// </summary>
        [Required]
        [Column("creado_en", TypeName = "DATETIME2(0)")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Lugar donde se realizó la evaluación
        /// </summary>
        [Required]
        [Column("lugar")]
        [StringLength(100)]
        public string Lugar { get; set; } = string.Empty;
    }
}