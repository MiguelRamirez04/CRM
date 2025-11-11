using System;
using back_cabs.CRM.enums;

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO para devolver información de una ejecución de orden de trabajo.
    /// Incluye datos del técnico y orden para contexto completo.
    /// </summary>
    public class EjecucionOrdenResponseDto
    {
        /// <summary>
        /// ID único de la ejecución.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la orden de trabajo asociada.
        /// </summary>
        public int OrdenId { get; set; }

        /// <summary>
        /// Tipo de ejecución: REMOTO o CAMPO.
        /// </summary>
        public TipoEjecucion TipoEjecucion { get; set; }

        /// <summary>
        /// ID del técnico asignado.
        /// </summary>
        public int TecnicoId { get; set; }

        /// <summary>
        /// Nombre del técnico (para display, obtenido de navegación).
        /// </summary>
        public string? TecnicoNombre { get; set; }

        /// <summary>
        /// Hora de inicio de la ejecución.
        /// </summary>
        public DateTime? HrInicio { get; set; }

        /// <summary>
        /// Hora de fin de la ejecución.
        /// </summary>
        public DateTime? HrFin { get; set; }

        /// <summary>
        /// Duración en minutos (calculado si HrInicio y HrFin existen).
        /// </summary>
        public int? DuracionMinutos { get; set; }

        /// <summary>
        /// Estado de la ejecución: EN_CURSO, FINALIZADA.
        /// </summary>
        public string EstadoEjecucion { get; set; } = "EN_CURSO";

        /// <summary>
        /// Comentarios sobre la ejecución.
        /// </summary>
        public string? Comentarios { get; set; }

        // Campos específicos para CAMPO
        /// <summary>
        /// ID del vehículo utilizado.
        /// </summary>
        public int? VehiculoId { get; set; }

        /// <summary>
        /// Placas del vehículo (para display).
        /// </summary>
        public string? VehiculoPlacas { get; set; }

        /// <summary>
        /// Kilometraje inicial.
        /// </summary>
        public int? KmInicial { get; set; }

        /// <summary>
        /// Kilometraje final.
        /// </summary>
        public int? KmFinal { get; set; }

        /// <summary>
        /// Kilómetros recorridos (calculado si KmInicial y KmFinal existen).
        /// </summary>
        public int? KmRecorridos { get; set; }

        // Campos específicos para REMOTO
        /// <summary>
        /// Herramientas utilizadas.
        /// </summary>
        public string? Herramientas { get; set; }

        /// <summary>
        /// Código de sesión remota.
        /// </summary>
        public string? CodigoSesion { get; set; }

        /// <summary>
        /// Contraseña de sesión remota.
        /// </summary>
        public string? ContrasenaSesion { get; set; }

        // ==========================================
        // CAMPOS ADICIONALES PARA EL NUEVO FLUJO
        // ==========================================

        /// <summary>
        /// Información del cliente de la orden
        /// </summary>
        public string? ClienteNombre { get; set; }

        /// <summary>
        /// Descripción de la orden de trabajo
        /// </summary>
        public string? OrdenDescripcion { get; set; }

        /// <summary>
        /// Prioridad de la orden
        /// </summary>
        public string? OrdenPrioridad { get; set; }

        /// <summary>
        /// Usuario que asignó la tarea
        /// </summary>
        public string? AsignadaPor { get; set; }

        /// <summary>
        /// Fecha de asignación de la tarea
        /// </summary>
        public DateTime? FechaAsignacion { get; set; }

        /// <summary>
        /// Número de notificaciones pendientes para esta ejecución
        /// </summary>
        public int NotificacionesPendientes { get; set; }

        /// <summary>
        /// Tiempo transcurrido desde el inicio (formateado)
        /// </summary>
        public string? TiempoTranscurrido { get; set; }

    }
}