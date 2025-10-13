using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.shared
{
    public class EvaluacionFotoRequestDto
    {  
        [Required(ErrorMessage = "El id del detalle es obligatorio")]
        public int DetalleId { get; set; }
        [Required(ErrorMessage = "El id del documento es obligatorio")]
        public int DocumentoId { get; set; }
        [StringLength(20, ErrorMessage = "El tipo no puede exceder los 20 caracteres")]
        public string? Tipo { get; set; }
        [StringLength(500, ErrorMessage ="La descripcion no puede exceder los 500 caracteres")]
        public string? Descripcion { get; set; }
        [Required(ErrorMessage ="La fecha de creacion es obligatorio")]
        public DateTime CreadoEn { get; set; }
    }
} 