using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Shared
{
    /// <summary>
    /// Entidad que representa una evaluación de servicio
    /// </summary>
    [Table("evaluaciones")]
    /*[Table("ejecuciones_orden", Schema = "ops")] */
    public class Evaluacion
    {
        /// <summary>
        /// Identificador único de la evaluación (autoincremental)
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID de la orden de trabajo evaluada
        /// </summary>
        [Required]
        [Column("orden_id")]
        public int OrdenId { get; set; }

        /// <summary>
        /// ID de la ejecución relacionada
        /// </summary>
        [Column("ejecucion_id")]
        public int? EjecucionId { get; set; }

        /// <summary>
        /// ID del cliente evaluado
        /// </summary>
        [Column("cliente_id")]
        public int? ClienteId { get; set; }

        /// <summary>
        /// ID del evaluador
        /// </summary>
        [Required]
        [Column("evaluador_id")]
        public int EvaluadorId { get; set; }

        /// <summary>
        /// Objetivo de la evaluación
        /// </summary>
        [Column("objetivo")]
        [StringLength(200)]
        public string? Objetivo { get; set; }

        /// <summary>
        /// Comentarios generales de la evaluación
        /// </summary>
        [Column("comentarios_generales")]
        public string? ComentariosGenerales { get; set; }

        /// <summary>
        /// Score total de calidad
        /// </summary>
        [Column("score_calidad_total")]
        public int? ScoreCalidadTotal { get; set; }

        /// <summary>
        /// Indica si requiere seguimiento
        /// </summary>
        [Required]
        [Column("requiere_seguimiento")]
        public bool RequiereSeguimiento { get; set; } = false;

        /// <summary>
        /// Notas del seguimiento
        /// </summary>
        [Column("seguimiento_notas")]
        public string? SeguimientoNotas { get; set; }

        /// <summary>
        /// Fecha y hora de creación
        /// </summary>
        [Required]
        [Column("creado_en", TypeName = "timestamp")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // =====================================================================
        // PROPIEDADES DE NAVEGACIÓN (para optimización de queries)
        // =====================================================================

        /// <summary>
        /// Detalles de la evaluación (relación 1:N)
        /// Permite cargar detalles con Include() para evitar N+1 queries
        /// </summary>
        public virtual ICollection<EvaluacionDetalle> Detalles { get; set; } = new List<EvaluacionDetalle>();

        /// <summary>
        /// Fotos de la evaluación a través de detalles (relación 1:N indirecta)
        /// Nota: Las fotos están relacionadas con DetalleId, no directamente con EvaluacionId
        /// Para cargar fotos: Include(e => e.Detalles).ThenInclude(d => d.Fotos)
        /// </summary>
        // public virtual ICollection<EvaluacionFoto> Fotos { get; set; } = new List<EvaluacionFoto>();
        // ⚠️ Comentado porque las fotos están relacionadas con DetalleId, no EvaluacionId
    }
}