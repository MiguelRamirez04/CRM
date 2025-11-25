using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;


namespace back_cabs.CRM.DTOs.shared{
    /// <summary>
    /// Dto para crear una nueva evaluacoin, continente
    /// aqui se ponen las validaciones de entrada
    /// </summary>
    public class EvaluacionRequestDto{
        [Required(ErrorMessage = "el ID de la orden de trabajo es obligatorio.")]
        public int OrdenId { get; set; }
        public int? EjecucionId { get; set; }
        public int? CLienteId { get; set; }

        [Required(ErrorMessage = "El id del evaluador es obligatrio.")]
        public int EvaluadorId { get; set; }
        [StringLength(200)]
        public string? Objetivo { get; set; }
        public string? ComentariosGenerales { get; set; }
        public int? ScoreCalidadTotal { get; set; }
        [Required]
        public bool RequiereSeguimiento { get; set; }
        public string? SeguimientoNotas { get; set; }
        public string? EvaluacionActualizada { get; set; }
        public string? NuevaEvaluacion { get; set; }

    }
}