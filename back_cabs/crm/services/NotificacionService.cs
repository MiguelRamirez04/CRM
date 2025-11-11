// =====================================================================================
// SERVICIO NOTIFICACIONES - NotificacionService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa la lógica de negocio para el sistema de notificaciones.
// Gestiona la creación, envío y gestión de notificaciones para usuarios.
//
// FUNCIONALIDADES:
// - Crear notificaciones
// - Obtener notificaciones por usuario
// - Marcar como leídas
// - Notificaciones automáticas para eventos del sistema
//
// =====================================================================================

using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.models;
using back_cabs.CRM.hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace back_cabs.CRM.services
{
    /// <summary>
    /// Servicio para gestión de notificaciones
    /// </summary>
    public class NotificacionService : INotificacionService
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly IHubContext<NotificacionesHub> _hubContext;
        private readonly ILogger<NotificacionService> _logger;

        public NotificacionService(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            IHubContext<NotificacionesHub> hubContext,
            ILogger<NotificacionService> logger)
        {
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea una nueva notificación
        /// </summary>
        public async Task<NotificacionDto> CrearNotificacionAsync(
            int usuarioId,
            string tipo,
            string titulo,
            string mensaje,
            string? datos = null,
            string prioridad = "MEDIA",
            string? accion = null)
        {
            try
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = usuarioId,
                    Tipo = tipo,
                    Titulo = titulo,
                    Mensaje = mensaje,
                    Datos = datos,
                    Prioridad = prioridad,
                    Accion = accion,
                    FechaCreacion = DateTime.UtcNow
                };

                _writeContext.Notificaciones.Add(notificacion);
                await _writeContext.SaveChangesAsync();

                // Crear DTO para enviar por SignalR
                var notificacionDto = MapToDto(notificacion);

                // Enviar notificación en tiempo real al usuario
                await _hubContext.Clients.Group($"usuario_{usuarioId}")
                    .SendAsync("RecibirNotificacion", notificacionDto);

                _logger.LogInformation("Notificación creada y enviada en tiempo real para usuario {UsuarioId}: {Tipo} - {Titulo}",
                    usuarioId, tipo, titulo);

                return notificacionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear notificación para usuario {UsuarioId}", usuarioId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene notificaciones de un usuario
        /// </summary>
        public async Task<List<NotificacionDto>> ObtenerNotificacionesAsync(
            int usuarioId,
            bool soloNoLeidas = false,
            int? limite = null)
        {
            try
            {
                var query = _readContext.Notificaciones
                    .Where(n => n.UsuarioId == usuarioId);

                if (soloNoLeidas)
                {
                    query = query.Where(n => !n.Leida);
                }

                query = query.OrderByDescending(n => n.FechaCreacion);

                if (limite.HasValue)
                {
                    query = query.Take(limite.Value);
                }

                var notificaciones = await query.ToListAsync();

                return notificaciones.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones para usuario {UsuarioId}", usuarioId);
                throw;
            }
        }

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        public async Task<bool> MarcarComoLeidaAsync(int notificacionId, int usuarioId)
        {
            try
            {
                var notificacion = await _writeContext.Notificaciones
                    .FirstOrDefaultAsync(n => n.Id == notificacionId && n.UsuarioId == usuarioId);

                if (notificacion == null)
                {
                    _logger.LogWarning("Notificación {NotificacionId} no encontrada para usuario {UsuarioId}",
                        notificacionId, usuarioId);
                    return false;
                }

                if (!notificacion.Leida)
                {
                    notificacion.MarcarComoLeida();
                    await _writeContext.SaveChangesAsync();

                    _logger.LogInformation("Notificación {NotificacionId} marcada como leída", notificacionId);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar notificación {NotificacionId} como leída", notificacionId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene el conteo de notificaciones no leídas
        /// </summary>
        public async Task<int> ObtenerConteoNoLeidasAsync(int usuarioId)
        {
            try
            {
                return await _readContext.Notificaciones
                    .CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteo de notificaciones no leídas para usuario {UsuarioId}", usuarioId);
                throw;
            }
        }

        // ==========================================
        // MÉTODOS DE NOTIFICACIONES AUTOMÁTICAS
        // ==========================================

        /// <summary>
        /// Notifica cuando se delega una tarea
        /// </summary>
        public async Task NotificarDelegacionAsync(int ejecucionId, int nuevoTecnicoId, string motivo)
        {
            try
            {
                // Obtener información de la ejecución
                var ejecucion = await _readContext.EjecucionesOrden
                    .Include(e => e.Orden)
                    .Include(e => e.Tecnico)
                    .FirstOrDefaultAsync(e => e.Id == ejecucionId);

                if (ejecucion == null) return;

                var datos = JsonSerializer.Serialize(new
                {
                    EjecucionId = ejecucionId,
                    OrdenId = ejecucion.OrdenId,
                    TecnicoAnterior = ejecucion.Tecnico?.Nombre + " " + ejecucion.Tecnico?.Apellido,
                    Motivo = motivo
                });

                await CrearNotificacionAsync(
                    nuevoTecnicoId,
                    TipoNotificacion.TAREA_DELEGADA,
                    "Nueva tarea delegada",
                    $"Se te ha delegado la orden #{ejecucion.OrdenId}. Motivo: {motivo}",
                    datos,
                    "ALTA",
                    $"/ejecuciones/{ejecucionId}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al notificar delegación de ejecución {EjecucionId}", ejecucionId);
            }
        }

        /// <summary>
        /// Notifica cuando se toma una tarea
        /// </summary>
        public async Task NotificarTareaTomadaAsync(int ordenId, int tecnicoId)
        {
            try
            {
                // Notificar a otros técnicos que la tarea ya no está disponible
                var otrosTecnicos = await _readContext.UsuariosAuth
                    .Where(u => u.Rol == "SOPORTE" && u.Id != tecnicoId && u.Activo)
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var tecnico in otrosTecnicos)
                {
                    await CrearNotificacionAsync(
                        tecnico,
                        TipoNotificacion.SISTEMA,
                        "Tarea no disponible",
                        $"La orden #{ordenId} ya fue tomada por otro técnico",
                        JsonSerializer.Serialize(new { OrdenId = ordenId }),
                        "BAJA"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al notificar tarea tomada {OrdenId}", ordenId);
            }
        }

        /// <summary>
        /// Crea recordatorios para tareas pendientes
        /// </summary>
        public async Task CrearRecordatoriosAsync()
        {
            try
            {
                // Tareas que llevan más de 2 horas sin asignar
                var tareasAntiguas = await _readContext.OrdenesTrabajo
                    .Where(o => (o.Estado == "CAPTURADA" || o.Estado == "ASIGNADA") &&
                               o.CreadoEn < DateTime.UtcNow.AddHours(-2))
                    .Include(o => o.CreadoPor)
                    .ToListAsync();

                foreach (var orden in tareasAntiguas)
                {
                    if (orden.CreadoPor != null)
                    {
                        await CrearNotificacionAsync(
                            orden.CreadoPor.Id,
                            TipoNotificacion.RECORDATORIO_PENDIENTE,
                            "Recordatorio: Tarea pendiente",
                            $"La orden #{orden.Id} lleva más de 2 horas sin asignar a un técnico",
                            JsonSerializer.Serialize(new { OrdenId = orden.Id }),
                            "MEDIA",
                            "/ordenes"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear recordatorios");
            }
        }

        /// <summary>
        /// Notifica cuando se finaliza una ejecución
        /// </summary>
        public async Task NotificarEjecucionFinalizadaAsync(int ejecucionId)
        {
            try
            {
                // Obtener información de la ejecución finalizada
                var ejecucion = await _readContext.EjecucionesOrden
                    .Include(e => e.Orden)
                    .Include(e => e.Tecnico)
                    .FirstOrDefaultAsync(e => e.Id == ejecucionId);

                if (ejecucion == null)
                {
                    _logger.LogWarning("Ejecución {EjecucionId} no encontrada para notificación de finalización", ejecucionId);
                    return;
                }

                // Notificar a supervisores (ADMINISTRACION y ADMINISTRADOR)
                var supervisores = await _readContext.UsuariosAuth
                    .Where(u => (u.Rol == "ADMINISTRACION" || u.Rol == "ADMINISTRADOR") && u.Activo)
                    .Select(u => u.Id)
                    .ToListAsync();

                var datos = JsonSerializer.Serialize(new
                {
                    EjecucionId = ejecucionId,
                    OrdenId = ejecucion.OrdenId,
                    TecnicoId = ejecucion.TecnicoId,
                    TecnicoNombre = ejecucion.Tecnico?.Nombre ?? "Desconocido"
                });

                foreach (var supervisorId in supervisores)
                {
                    await CrearNotificacionAsync(
                        supervisorId,
                        TipoNotificacion.TAREA_FINALIZADA,
                        "Ejecución finalizada",
                        $"La orden #{ejecucion.OrdenId} ha sido completada por {ejecucion.Tecnico?.Nombre ?? "técnico"}",
                        datos,
                        "MEDIA",
                        $"/ordenes/{ejecucion.OrdenId}"
                    );
                }

                _logger.LogInformation("Notificación de finalización enviada para ejecución {EjecucionId}", ejecucionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al notificar finalización de ejecución {EjecucionId}", ejecucionId);
            }
        }

        /// <summary>
        /// Elimina una notificación
        /// </summary>
        public async Task<bool> EliminarNotificacionAsync(int notificacionId)
        {
            try
            {
                var notificacion = await _writeContext.Notificaciones.FindAsync(notificacionId);

                if (notificacion == null)
                {
                    _logger.LogWarning("Notificación {NotificacionId} no encontrada para eliminación", notificacionId);
                    return false;
                }

                _writeContext.Notificaciones.Remove(notificacion);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Notificación {NotificacionId} eliminada exitosamente", notificacionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar notificación {NotificacionId}", notificacionId);
                throw;
            }
        }

        private NotificacionDto MapToDto(Notificacion notificacion)
        {
            return new NotificacionDto
            {
                Id = notificacion.Id,
                UsuarioId = notificacion.UsuarioId,
                Tipo = notificacion.Tipo,
                Titulo = notificacion.Titulo,
                Mensaje = notificacion.Mensaje,
                Datos = notificacion.Datos,
                Leida = notificacion.Leida,
                FechaCreacion = notificacion.FechaCreacion,
                FechaLectura = notificacion.FechaLectura,
                Prioridad = notificacion.Prioridad,
                Accion = notificacion.Accion
            };
        }
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