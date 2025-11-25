using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.Request
{
    /// <summary>
    /// DTO para subir una foto a una reparación.
    /// </summary>
    public class ReparacionFotoUploadRequestDto
    {
        /// <summary>
        /// Archivo de imagen a subir (JPEG, PNG, WebP, GIF).
        /// </summary>
        [Required(ErrorMessage = "El archivo es obligatorio.")]
        public IFormFile Archivo { get; set; } = null!;

        /// <summary>
        /// Etapa de la reparación (ej: "Recibido", "Proceso", "Finalizado").
        /// </summary>
        [MaxLength(20, ErrorMessage = "La etapa no puede exceder 20 caracteres.")]
        public string? Etapa { get; set; }

        /// <summary>
        /// Descripción opcional de la foto.
        /// </summary>
        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
        public string? Descripcion { get; set; }
    }
}
