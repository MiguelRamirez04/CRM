using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace back_cabs.CRM.DTOs.shared
{
    /// <summary>
    /// DTO para subir una foto de evaluación.
    /// </summary>
    public class EvaluacionFotoRequestDto
    {  
        /// <summary>
        /// ID del detalle de evaluación al que pertenece la foto.
        /// </summary>
        [Required(ErrorMessage = "El ID del detalle es obligatorio.")]
        public int DetalleId { get; set; }

        /// <summary>
        /// Archivo de imagen a subir (JPEG, PNG, WebP, GIF).
        /// </summary>
        [Required(ErrorMessage = "El archivo es obligatorio.")]
        public IFormFile Archivo { get; set; } = null!;

        /// <summary>
        /// Tipo de foto (ej: "Antes", "Después", "Daño").
        /// </summary>
        [StringLength(20, ErrorMessage = "El tipo no puede exceder 20 caracteres.")]
        public string? Tipo { get; set; }

        /// <summary>
        /// Descripción opcional de la foto.
        /// </summary>
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
        public string? Descripcion { get; set; }
    }
} 