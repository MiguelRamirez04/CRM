// =====================================================================================
// SIGNALR HUB NOTIFICACIONES - NotificacionesHub.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa el hub de SignalR para notificaciones en tiempo real.
// Permite enviar notificaciones push a usuarios conectados.
//
// FUNCIONALIDADES:
// - Conexión de usuarios por ID
// - Envío de notificaciones individuales
// - Broadcast a grupos de usuarios
//
// =====================================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace back_cabs.CRM.hubs
{
    /// <summary>
    /// Hub de SignalR para notificaciones en tiempo real
    /// </summary>
    public class NotificacionesHub : Hub
    {
        private readonly ILogger<NotificacionesHub> _logger;

        public NotificacionesHub(ILogger<NotificacionesHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Se ejecuta cuando un cliente se conecta al hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Cliente conectado al hub de notificaciones: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Se ejecuta cuando un cliente se desconecta del hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception != null)
            {
                _logger.LogWarning(exception, "Cliente desconectado con error: {ConnectionId}", Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("Cliente desconectado del hub: {ConnectionId}", Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Registra al usuario en un grupo basado en su ID
        /// </summary>
        public async Task RegistrarUsuario(string usuarioIdStr)
        {
            try
            {
                _logger.LogInformation("🔍 Recibido usuarioId como string: '{Valor}'", usuarioIdStr);

                if (!int.TryParse(usuarioIdStr, out int usuarioId))
                {
                    throw new ArgumentException($"El ID de usuario '{usuarioIdStr}' no es un número válido", nameof(usuarioIdStr));
                }

                _logger.LogInformation("🔍 Convertido a int: {Valor}", usuarioId);

                if (usuarioId <= 0)
                {
                    throw new ArgumentException("El ID de usuario debe ser un número positivo", nameof(usuarioIdStr));
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, $"usuario_{usuarioId}");
                _logger.LogInformation("✅ Usuario {UsuarioId} registrado en grupo de notificaciones (ConnectionId: {ConnectionId})", 
                    usuarioId, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al registrar usuario en grupo: {Mensaje}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Desregistra al usuario de su grupo
        /// </summary>
        public async Task DesregistrarUsuario(string usuarioIdStr)
        {
            try
            {
                if (!int.TryParse(usuarioIdStr, out int usuarioId))
                {
                    throw new ArgumentException($"El ID de usuario '{usuarioIdStr}' no es un número válido", nameof(usuarioIdStr));
                }

                if (usuarioId <= 0)
                {
                    throw new ArgumentException("El ID de usuario debe ser un número positivo", nameof(usuarioIdStr));
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"usuario_{usuarioId}");
                _logger.LogInformation("✅ Usuario {UsuarioId} desregistrado del grupo de notificaciones (ConnectionId: {ConnectionId})", 
                    usuarioId, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al desregistrar usuario del grupo: {Mensaje}", ex.Message);
                throw;
            }
        }
    }
}