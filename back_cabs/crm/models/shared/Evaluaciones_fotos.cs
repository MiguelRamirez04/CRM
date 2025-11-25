using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Shared
{
    /// <summary>
    /// Entidad que representa una foto asociada a un detalle de evaluación
    /// </summary>
    [Table("evaluacion_fotos")]
    public class EvaluacionFoto
    {
        /// <summary>
        /// Identificador único de la foto (autoincremental)
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID del detalle de evaluación al que pertenece la foto
        /// </summary>
        [Required]
        [Column("detalle_id")]
        public int DetalleId { get; set; }

        /// <summary>
        /// ID del documento/foto
        /// </summary>
        [Required]
        [Column("documento_id")]
        public int DocumentoId { get; set; }

        /// <summary>
        /// Tipo de foto (antes, durante, después, etc.)
        /// </summary>
        [Column("tipo")]
        [StringLength(20)]
        public string? Tipo { get; set; }

        /// <summary>
        /// Descripción de la foto
        /// </summary>
        [Column("descripcion")]
        [StringLength(500)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Fecha y hora de creación
        /// </summary>
        [Required]
        [Column("creado_en", TypeName = "DATETIME2(0)")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // --- Propiedades de Navegación ---
        
        /// <summary>
        /// Navegación al detalle de evaluación asociado
        /// </summary>
        [ForeignKey("DetalleId")]
        public virtual EvaluacionDetalle? Detalle { get; set; }

        /// <summary>
        /// Navegación al documento/archivo físico
        /// </summary>
        [ForeignKey("DocumentoId")]
        public virtual back_cabs.CRM.models.Files.FilesDocumento? Documento { get; set; }
    }
}