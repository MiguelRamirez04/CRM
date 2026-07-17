using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Shared

{


    [Table("viatico_gasto_detalle")]
    public class GastoViaticoDetalle
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("viatico_id")]
        public int ViaticoId { get; set; }

        [Required]
        [Column("tipo_gasto")]
        [StringLength(50)]
        public string TipoGasto { get; set; } = string.Empty; 

        [Required]
        [Column("monto")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto debe de ser un valor positivo ")]
        public decimal Monto { get; set; }

        [Column("descripcion")]
        [StringLength(500)]
        public string? Descripcion { get; set; }

    }
}