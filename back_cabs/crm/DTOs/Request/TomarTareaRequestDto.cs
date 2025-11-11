// =====================================================================================
// DTO TOMAR TAREA - TomarTareaRequestDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el DTO para la solicitud de tomar una tarea pendiente.
// Cuando un técnico toma una tarea, se crea automáticamente una ejecución.
//
// CUÁNDO USARLO:
// - Endpoint para que técnicos tomen tareas disponibles
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.Request
{
    /// <summary>
    /// DTO para solicitar tomar una tarea pendiente
    /// </summary>
    public class TomarTareaRequestDto
    {
        /// <summary>
        /// ID de la orden de trabajo a tomar
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la orden debe ser mayor a 0")]
        public int OrdenId { get; set; }

        /// <summary>
        /// Tipo de ejecución a crear (CAMPO o REMOTO)
        /// </summary>
        [Required]
        [RegularExpression("^(CAMPO|REMOTO)$", ErrorMessage = "El tipo de ejecución debe ser CAMPO o REMOTO")]
        public string TipoEjecucion { get; set; } = "CAMPO";

        /// <summary>
        /// ID del vehículo (requerido si TipoEjecucion = CAMPO)
        /// </summary>
        public int? VehiculoId { get; set; }

        /// <summary>
        /// Kilometraje inicial (requerido si TipoEjecucion = CAMPO)
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje inicial debe ser mayor o igual a 0")]
        public int? KmInicial { get; set; }

        /// <summary>
        /// Herramientas necesarias (opcional para REMOTO)
        /// </summary>
        [StringLength(500, ErrorMessage = "Las herramientas no pueden exceder 500 caracteres")]
        public string? Herramientas { get; set; }

        /// <summary>
        /// Código de sesión remota (opcional para REMOTO)
        /// </summary>
        [StringLength(100, ErrorMessage = "El código de sesión no puede exceder 100 caracteres")]
        public string? CodigoSesion { get; set; }

        /// <summary>
        /// Contraseña de sesión remota (opcional para REMOTO)
        /// </summary>
        [StringLength(100, ErrorMessage = "La contraseña de sesión no puede exceder 100 caracteres")]
        public string? ContrasenaSesion { get; set; }

        /// <summary>
        /// Comentarios iniciales sobre la tarea
        /// </summary>
        [StringLength(1000, ErrorMessage = "Los comentarios iniciales no pueden exceder 1000 caracteres")]
        public string? Comentarios { get; set; }
    }
}