// back_cabs.CRM.DTOs.shared/DtoEvaDetallesRequest.cs

using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.shared
{
    public class DtoEvaDetallesRequest
    {
        // El Id se elimina. El cliente no debe enviarlo al crear.

        [Required(ErrorMessage = "El ID de la evaluación padre es obligatorio.")]
        public int EvaluacionId { get; set; }

        [Required(ErrorMessage = "La fase es obligatoria.")]
        [StringLength(10, ErrorMessage = "La fase no puede exceder los 10 caracteres.")]
        public string Fase { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string? Sugerencias { get; set; }

        // CORREGIDO: El nombre ahora es ScoreFase para coincidir con el modelo.
        public int? ScoreFase { get; set; }

        // CORREGIDO: El nombre ahora es EvidenciaNota.
        // Se quitó [Required] porque en el modelo es nullable (string?).
        public string? EvidenciaNota { get; set; }

        // CreadoEn se elimina. El servidor debe gestionar esta fecha.

        [Required(ErrorMessage = "El campo de lugar es obligatorio.")]
        [StringLength(100, ErrorMessage = "El lugar no puede exceder los 100 caracteres.")]
        public string Lugar { get; set; } = string.Empty;
    }
}