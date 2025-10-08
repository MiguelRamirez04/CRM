using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Shared
{
    [Table("evaluaciones_fotos")]
    public class EvaluacionFoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("detalle_id")]
        public int DetalleId { get; set; }

        [Required]
        [Column("documento_id")]
        public int DocumentoId { get; set; }

        [Column("tipo")]
        [StringLength(20)]
        public string? Tipo { get; set; }

        [Column("descripcion")]
        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }
}