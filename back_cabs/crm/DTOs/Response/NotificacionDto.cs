// =====================================================================================
// DTO NOTIFICACIÓN - NotificacionDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el DTO para notificaciones del sistema.
// Las notificaciones informan sobre eventos importantes en el flujo de trabajo.
//
// CUÁNDO USARLO:
// - Notificaciones de delegación de tareas
// - Alertas de tareas pendientes
// - Recordatorios del sistema
//
// =====================================================================================

using System;

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO que representa una notificación del sistema
    /// </summary>
    public class NotificacionDto
    {
        /// <summary>
        /// ID único de la notificación
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario destinatario
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Tipo de notificación
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Título de la notificación
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje detallado
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Datos adicionales en JSON (IDs relacionados, etc.)
        /// </summary>
        public string? Datos { get; set; }

        /// <summary>
        /// Indica si la notificación ha sido leída
        /// </summary>
        public bool Leida { get; set; } = false;

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de lectura (si aplica)
        /// </summary>
        public DateTime? FechaLectura { get; set; }

        /// <summary>
        /// Nivel de prioridad (BAJA, MEDIA, ALTA, URGENTE)
        /// </summary>
        public string Prioridad { get; set; } = "MEDIA";

        /// <summary>
        /// Acción sugerida (URL o identificador de acción)
        /// </summary>
        public string? Accion { get; set; }
    }

    /// <summary>
    /// Tipos de notificación disponibles
    /// </summary>
    public static class TipoNotificacion
    {
        public const string TAREA_DELEGADA = "TAREA_DELEGADA";
        public const string TAREA_FINALIZADA = "TAREA_FINALIZADA";
        public const string RECORDATORIO_PENDIENTE = "RECORDATORIO_PENDIENTE";
        public const string SISTEMA = "SISTEMA";
    }
}