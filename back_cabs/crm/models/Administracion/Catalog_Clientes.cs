using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Catalog
{
    [Table("clientes", Schema = "catalog")]
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        public int? LegacyClientId { get; set; }

        [StringLength(200)]
        public string? NombreComercial { get; set; }

        [StringLength(100)]
        public string? Nombre { get; set; }

        [StringLength(100)]
        public string? Apellido { get; set; }

        [StringLength(13)]
        public string? RFC { get; set; }

        [StringLength(50)]
        public string? EstadoFiscal { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(150)]
        public string? Email { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? DireccionJson { get; set; }
    }
}