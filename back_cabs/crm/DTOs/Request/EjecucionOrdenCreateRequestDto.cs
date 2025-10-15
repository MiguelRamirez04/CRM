using System;
using System.ComponentModel.DataAnnotations;
using back_cabs.CRM.enums;

namespace back_cabs.CRM.DTOs.Request
{
    /// <summary>
    /// DTO para crear una nueva ejecución de orden de trabajo.
    /// Incluye validaciones para asegurar datos correctos.
    /// </summary>
    public class EjecucionOrdenCreateRequestDto
    {
        /// <summary>
        /// ID de la orden de trabajo a ejecutar (obligatorio).
        /// </summary>
        [Required(ErrorMessage = "El ID de la orden es obligatorio.")]
        public int OrdenId { get; set; }

        /// <summary>
        /// Tipo de ejecución: REMOTO o CAMPO (obligatorio).
        /// </summary>
        [Required(ErrorMessage = "El tipo de ejecución es obligatorio.")]
        public TipoEjecucion TipoEjecucion { get; set; }

        /// <summary>
        /// ID del técnico asignado (debe ser usuario con rol SOPORTE, obligatorio).
        /// </summary>
        [Required(ErrorMessage = "El ID del técnico es obligatorio.")]
        public int TecnicoId { get; set; }

        /// <summary>
        /// Hora de inicio de la ejecución (opcional, se puede establecer al iniciar).
        /// </summary>
        public DateTime? HrInicio { get; set; }

        /// <summary>
        /// Comentarios adicionales sobre la ejecución (opcional).
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Los comentarios no pueden exceder 1000 caracteres.")]
        public string? Comentarios { get; set; }

        // Campos específicos para CAMPO (Visita)
        /// <summary>
        /// ID del vehículo utilizado (solo para tipo CAMPO, opcional).
        /// </summary>
        public int? VehiculoId { get; set; }

        /// <summary>
        /// Kilometraje inicial del vehículo (solo para tipo CAMPO, opcional).
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje inicial debe ser positivo.")]
        public int? KmInicial { get; set; }

        // Campos específicos para REMOTO (Sesión)
        /// <summary>
        /// Herramientas utilizadas (solo para tipo REMOTO, opcional).
        /// </summary>
        [MaxLength(100, ErrorMessage = "Las herramientas no pueden exceder 100 caracteres.")]
        public string? Herramientas { get; set; }

        /// <summary>
        /// Código de sesión remota (solo para tipo REMOTO, opcional).
        /// </summary>
        [MaxLength(100, ErrorMessage = "El código de sesión no puede exceder 100 caracteres.")]
        public string? CodigoSesion { get; set; }

        /// <summary>
        /// Contraseña de sesión remota (solo para tipo REMOTO, opcional).
        /// </summary>
        [MaxLength(100, ErrorMessage = "La contraseña de sesión no puede exceder 100 caracteres.")]
        public string? ContrasenaSesion { get; set; }
    }
}

















