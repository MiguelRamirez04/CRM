// =====================================================================================
// INTERFAZ NOTIFICACION SERVICE - INotificacionService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTA INTERFAZ?
// Define el contrato para el servicio de notificaciones.
// Especifica los métodos que debe implementar cualquier servicio de notificaciones.
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;

namespace back_cabs.CRM.services
{
    /// <summary>
    /// Interfaz para el servicio de notificaciones
    /// </summary>
    public interface INotificacionService
    {
        /// <summary>
        /// Obtiene todas las notificaciones de un usuario
        /// </summary>
        Task<List<NotificacionDto>> ObtenerNotificacionesAsync(int usuarioId, bool soloNoLeidas = false, int? limite = null);

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        Task<bool> MarcarComoLeidaAsync(int notificacionId, int usuarioId);

        /// <summary>
        /// Elimina una notificación
        /// </summary>
        Task<bool> EliminarNotificacionAsync(int notificacionId);

        /// <summary>
        /// Obtiene el conteo de notificaciones no leídas de un usuario
        /// </summary>
        Task<int> ObtenerConteoNoLeidasAsync(int usuarioId);

        /// <summary>
        /// Notifica delegación de tarea
        /// </summary>
        Task NotificarDelegacionAsync(int ejecucionId, int nuevoTecnicoId, string motivo);

        /// <summary>
        /// Notifica que una tarea fue tomada
        /// </summary>
        Task NotificarTareaTomadaAsync(int ordenId, int tecnicoId);

        /// <summary>
        /// Notifica que una ejecución fue finalizada
        /// </summary>
        Task NotificarEjecucionFinalizadaAsync(int ejecucionId);
    }
}