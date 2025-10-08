using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Recepcion
{
    [Table("ejecuciones_orden")]
    public class EjecucionOrden
    {
        [Key]
        public int Id { get; set; }

        public int OrdenId { get; set; }

        [StringLength(20)]
        public string TipoEjecucion { get; set; } = string.Empty;

        public int TecnicoId { get; set; }

        public DateTime? HrInicio { get; set; }
        public DateTime? HrFin { get; set; }

        public string? Comentarios { get; set; }

        // CAMPO
        public int? VehiculoId { get; set; }
        public int? KmInicial { get; set; }
        public int? KmFinal { get; set; }

        // REMOTO
        [StringLength(100)]
        public string? Herramientas { get; set; }

        [StringLength(100)]
        public string? CodigoSesion { get; set; }

        [StringLength(100)]
        public string? ContrasenaSesion { get; set; }
    }
}