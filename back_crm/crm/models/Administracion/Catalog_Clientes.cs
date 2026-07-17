using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Administracion
{
    [Table("catalog_clientes")]
    public class Catalog_Clientes
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("legacy_client_id")]
        public int? LegacyClientId { get; set; }

        [Column("nombre_comercial")]
        [StringLength(200)]
        public string? NombreComercial { get; set; }

        [Column("nombre")]
        [StringLength(100)]
        public string? Nombre { get; set; }

        [Column("apellido")]
        [StringLength(100)]
        public string? Apellido { get; set; }

        [Column("rfc")]
        [StringLength(13)]
        public string? RFC { get; set; }

        [Column("estado_fiscal")]
        [StringLength(50)]
        public string? EstadoFiscal { get; set; }

        [Column("telefono")]
        [StringLength(20)]
        public string? Telefono { get; set; }

        [Column("email")]
        [StringLength(150)]
        public string? Email { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("created_at", TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("direccion_json")]
        public string? DireccionJson { get; set; }
    }
}