using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.enums;
using back_cabs.CRM.Interfaces.Recepcion;
using back_cabs.CRM.models.Soporte;
using back_cabs.CRM.services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.services.shared
{
    /// <summary>
    /// Servicio para gestionar ejecuciones de órdenes de trabajo.
    /// Maneja operaciones CRUD y lógica de delegación entre técnicos.
    /// </summary>
    public class EjecucionOrdenService
    {
        private readonly IEjecucionOrdenRepository _ejecucionRepository;
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly UsuarioAuthService _usuarioAuthService;
        private readonly ILogger<EjecucionOrdenService> _logger;
        private readonly INotificacionService _notificacionService;

        public EjecucionOrdenService(
            IEjecucionOrdenRepository ejecucionRepository,
            WriteContext writeContext,
            ReadOnlyContext readContext,
            UsuarioAuthService usuarioAuthService,
            INotificacionService notificacionService,
            ILogger<EjecucionOrdenService> logger)
        {
            _ejecucionRepository = ejecucionRepository ?? throw new ArgumentNullException(nameof(ejecucionRepository));
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _usuarioAuthService = usuarioAuthService ?? throw new ArgumentNullException(nameof(usuarioAuthService));
            _notificacionService = notificacionService ?? throw new ArgumentNullException(nameof(notificacionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea una nueva ejecución de orden de trabajo.
        /// Valida que el técnico tenga rol SOPORTE, la orden exista, 
        /// y los campos específicos según el tipo de ejecución.
        /// </summary>
        /// <param name="dto">Datos de la ejecución a crear.</param>
        /// <returns>DTO de respuesta con la ejecución creada.</returns>
        /// <exception cref="ArgumentException">Si las validaciones fallan.</exception>
        public async Task<EjecucionOrdenResponseDto> CreateEjecucionAsync(EjecucionOrdenCreateRequestDto dto)
        {
            _logger.LogInformation("Creando nueva ejecución para orden {OrdenId} con técnico {TecnicoId}", dto.OrdenId, dto.TecnicoId);

            // 1. Validar que la orden exista
            var orden = await _readContext.OrdenesTrabajo
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == dto.OrdenId);
            
            if (orden == null)
            {
                _logger.LogWarning("Orden {OrdenId} no encontrada", dto.OrdenId);
                throw new ArgumentException($"La orden de trabajo con ID {dto.OrdenId} no existe.", nameof(dto.OrdenId));
            }

            // 2. Validar que el técnico exista y tenga rol SOPORTE
            var tecnico = await _readContext.UsuariosAuth
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == dto.TecnicoId);
            
            if (tecnico == null)
            {
                _logger.LogWarning("Técnico {TecnicoId} no encontrado", dto.TecnicoId);
                throw new ArgumentException($"El técnico con ID {dto.TecnicoId} no existe.", nameof(dto.TecnicoId));
            }

            // Solo permitir usuarios con rol SOPORTE para crear ejecuciones
            if (tecnico.Rol != RolUsuario.SOPORTE.ToString())
            {
                _logger.LogWarning("Usuario {TecnicoId} no tiene rol SOPORTE para crear ejecuciones (tiene {Rol})", dto.TecnicoId, tecnico.Rol);
                throw new ArgumentException($"El usuario {tecnico.Nombre} no tiene rol SOPORTE.", nameof(dto.TecnicoId));
            }

            // 3. Validaciones específicas según tipo de ejecución
            if (dto.TipoEjecucion == TipoEjecucion.CAMPO)
            {
                // Para ejecuciones de CAMPO, el vehículo es OBLIGATORIO
                if (!dto.VehiculoId.HasValue)
                {
                    _logger.LogWarning("Intentando crear ejecución CAMPO sin vehículo");
                    throw new ArgumentException("Las ejecuciones de tipo CAMPO requieren un vehículo.", nameof(dto.VehiculoId));
                }

                // Validar que el vehículo exista y esté activo
                var vehiculo = await _readContext.Vehiculos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Id == dto.VehiculoId.Value);

                if (vehiculo == null)
                {
                    _logger.LogWarning("Vehículo {VehiculoId} no encontrado", dto.VehiculoId);
                    throw new ArgumentException($"El vehículo con ID {dto.VehiculoId.Value} no existe.", nameof(dto.VehiculoId));
                }

                if (!vehiculo.Activo)
                {
                    _logger.LogWarning("Vehículo {VehiculoId} está inactivo", dto.VehiculoId);
                    throw new ArgumentException($"El vehículo {vehiculo.Placas} no está activo.", nameof(dto.VehiculoId));
                }

                // KmInicial es opcional, pero si se proporciona debe ser válido
                if (dto.KmInicial.HasValue && dto.KmInicial.Value < 0)
                {
                    _logger.LogWarning("KmInicial negativo: {KmInicial}", dto.KmInicial);
                    throw new ArgumentException("El kilometraje inicial debe ser mayor o igual a cero.", nameof(dto.KmInicial));
                }
            }
            else if (dto.TipoEjecucion == TipoEjecucion.REMOTO)
            {
                // Para REMOTO, NO se deben incluir datos de vehículo ni kilometraje
                if (dto.VehiculoId.HasValue || dto.KmInicial.HasValue)
                {
                    _logger.LogWarning("Intentando crear ejecución REMOTO con datos de vehículo");
                    throw new ArgumentException("Las ejecuciones de tipo REMOTO no deben incluir datos de vehículo o kilometraje.", nameof(dto.TipoEjecucion));
                }
            }

            // 4. Crear la ejecución con transacción
            var strategy = _writeContext.Database.CreateExecutionStrategy();

            var ejecucionResult = await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _writeContext.Database.BeginTransactionAsync();
                try
                {
                    // Crear la entidad con datos validados
                    var ejecucion = new EjecucionOrden
                    {
                        OrdenId = dto.OrdenId,
                        TipoEjecucion = dto.TipoEjecucion,
                        TecnicoId = dto.TecnicoId,
                        HrInicio = dto.HrInicio ?? DateTime.Now, // Si no se proporciona, usar NOW
                        Comentarios = dto.Comentarios,
                        // Campos específicos CAMPO
                        VehiculoId = dto.TipoEjecucion == TipoEjecucion.CAMPO ? dto.VehiculoId : null,
                        KmInicial = dto.TipoEjecucion == TipoEjecucion.CAMPO ? dto.KmInicial : null,
                        // Campos específicos REMOTO
                        Herramientas = dto.TipoEjecucion == TipoEjecucion.REMOTO ? dto.Herramientas : null,
                        CodigoSesion = dto.TipoEjecucion == TipoEjecucion.REMOTO ? dto.CodigoSesion : null,
                        ContrasenaSesion = dto.TipoEjecucion == TipoEjecucion.REMOTO ? dto.ContrasenaSesion : null
                    };

                    // ✅ Repository Pattern: crear ejecución
                    var ejecucionCreada = await _ejecucionRepository.CreateAsync(ejecucion);
                    await transaction.CommitAsync();

                    _logger.LogInformation("Ejecución {EjecucionId} creada exitosamente para orden {OrdenId}", ejecucionCreada.Id, dto.OrdenId);

                    // Recargar con navegaciones para mapear correctamente
                    var ejecucionCompleta = await _ejecucionRepository.GetByIdAsync(ejecucionCreada.Id);

                    return MapToResponseDto(ejecucionCompleta!);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al crear ejecución para orden {OrdenId}", dto.OrdenId);
                    throw;
                }
            });

            return ejecucionResult;
        }

        /// <summary>
        /// Obtiene una ejecución específica por ID.
        /// </summary>
        /// <param name="id">ID de la ejecución.</param>
        /// <returns>DTO de respuesta o null si no existe.</returns>
        public async Task<EjecucionOrdenResponseDto?> GetEjecucionByIdAsync(int id)
        {
            _logger.LogInformation("Consultando ejecución con ID {EjecucionId}", id);

            // ✅ Repository Pattern: obtener ejecución con relaciones
            var ejecucion = await _ejecucionRepository.GetByIdAsync(id);

            if (ejecucion == null)
            {
                _logger.LogWarning("Ejecución con ID {EjecucionId} no encontrada", id);
                return null;
            }

            return MapToResponseDto(ejecucion);
        }

        /// <summary>
        /// Lista ejecuciones con filtros opcionales.
        /// </summary>
        /// <param name="ordenId">Filtrar por orden.</param>
        /// <param name="tecnicoId">Filtrar por técnico.</param>
        /// <param name="tipoEjecucion">Filtrar por tipo.</param>
        /// <param name="fechaDesde">Filtrar desde fecha.</param>
        /// <param name="fechaHasta">Filtrar hasta fecha.</param>
        /// <returns>Lista de DTOs de respuesta.</returns>
        public async Task<List<EjecucionOrdenResponseDto>> GetEjecucionesAsync(
            int? ordenId = null,
            int? tecnicoId = null,
            TipoEjecucion? tipoEjecucion = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            _logger.LogInformation("Consultando ejecuciones con filtros: OrdenId={OrdenId}, TecnicoId={TecnicoId}, Tipo={Tipo}, Desde={Desde}, Hasta={Hasta}",
                ordenId, tecnicoId, tipoEjecucion, fechaDesde, fechaHasta);

            // ✅ Repository Pattern: obtener ejecuciones con filtros e incluir relaciones
            var ejecuciones = await _readContext.EjecucionesOrden
                .Include(e => e.Tecnico)
                .Include(e => e.Vehiculo)
                .Include(e => e.Orden)
                    .ThenInclude(o => o!.CreadoPor) // Nueva navegación para obtener datos de la orden
                .Where(e => !ordenId.HasValue || e.OrdenId == ordenId)
                .Where(e => !tecnicoId.HasValue || e.TecnicoId == tecnicoId)
                .Where(e => !tipoEjecucion.HasValue || e.TipoEjecucion == tipoEjecucion)
                .Where(e => !fechaDesde.HasValue || (e.HrInicio.HasValue && e.HrInicio >= fechaDesde))
                .Where(e => !fechaHasta.HasValue || (e.HrInicio.HasValue && e.HrInicio <= fechaHasta))
                .OrderByDescending(e => e.HrInicio)
                .ToListAsync();

            var result = new List<EjecucionOrdenResponseDto>();
            foreach (var ejecucion in ejecuciones)
            {
                var dto = MapToResponseDto(ejecucion);

                // Poblar campos adicionales de la orden
                if (ejecucion.Orden != null)
                {
                    dto.ClienteNombre = ejecucion.Orden.NombreClienteCompleto;
                    dto.OrdenDescripcion = ejecucion.Orden.Notas;
                    dto.OrdenPrioridad = ejecucion.Orden.Prioridad?.ToString();
                    dto.AsignadaPor = ejecucion.Orden.CreadoPor != null
                        ? $"{ejecucion.Orden.CreadoPor.Nombre} {ejecucion.Orden.CreadoPor.Apellido}"
                        : null;
                    dto.FechaAsignacion = ejecucion.Orden.CreadoEn;
                }

                // Calcular tiempo transcurrido si está en curso
                if (ejecucion.HrInicio.HasValue && !ejecucion.HrFin.HasValue)
                {
                    var tiempoTranscurrido = DateTime.UtcNow - ejecucion.HrInicio.Value;
                    if (tiempoTranscurrido.TotalHours < 1)
                    {
                        dto.TiempoTranscurrido = $"{(int)tiempoTranscurrido.TotalMinutes}m";
                    }
                    else if (tiempoTranscurrido.TotalDays < 1)
                    {
                        dto.TiempoTranscurrido = $"{(int)tiempoTranscurrido.TotalHours}h {(int)tiempoTranscurrido.Minutes}m";
                    }
                    else
                    {
                        dto.TiempoTranscurrido = $"{(int)tiempoTranscurrido.TotalDays}d {(int)tiempoTranscurrido.Hours}h";
                    }
                }

                // TODO: Calcular notificaciones pendientes
                dto.NotificacionesPendientes = 0;

                result.Add(dto);
            }

            _logger.LogInformation("Se encontraron {Count} ejecuciones", result.Count);
            return result;
        }

        /// <summary>
        /// Delega una ejecución a otro técnico (cambia tecnico_id).
        /// Registra el cambio en comentarios.
        /// </summary>
        /// <param name="ejecucionId">ID de la ejecución.</param>
        /// <param name="nuevoTecnicoId">ID del nuevo técnico.</param>
        /// <param name="usuarioActualId">ID del usuario que delega.</param>
        /// <param name="motivo">Motivo de la delegación.</param>
        /// <exception cref="ArgumentException">Si el nuevo técnico no es válido.</exception>
        /// <exception cref="KeyNotFoundException">Si la ejecución no existe.</exception>
        public async Task DelegateEjecucionAsync(int ejecucionId, int nuevoTecnicoId, int usuarioActualId, string motivo)
        {
            _logger.LogInformation("Delegando ejecución {EjecucionId} de usuario {UsuarioActualId} a técnico {NuevoTecnicoId}", ejecucionId, usuarioActualId, nuevoTecnicoId);

            // Validar nuevo técnico existe
            var nuevoTecnico = await _readContext.UsuariosAuth.FindAsync(nuevoTecnicoId);
            if (nuevoTecnico == null)
                throw new ArgumentException("El nuevo técnico no existe.", nameof(nuevoTecnicoId));

            // Obtener ejecución
            var ejecucion = await _writeContext.EjecucionesOrden.FindAsync(ejecucionId);
            if (ejecucion == null)
                throw new KeyNotFoundException($"Ejecución con ID {ejecucionId} no encontrada.");

            // Evitar delegación a sí mismo
            if (ejecucion.TecnicoId == nuevoTecnicoId)
                throw new ArgumentException("No se puede delegar a sí mismo.", nameof(nuevoTecnicoId));

            // ✅ Repository Pattern: delegar ejecución con validaciones y auditoría
            await _ejecucionRepository.DelegateAsync(ejecucionId, nuevoTecnicoId, usuarioActualId);

            // Notificar al nuevo técnico sobre la delegación
            await _notificacionService.NotificarDelegacionAsync(ejecucionId, nuevoTecnicoId, motivo);

            _logger.LogInformation("Ejecución {EjecucionId} delegada exitosamente a técnico {NuevoTecnicoId}", ejecucionId, nuevoTecnicoId);
        }

        /// <summary>
        /// Actualiza campos de una ejecución (ej. finalizar con HrFin, KmFinal).
        /// Solo el técnico asignado puede actualizar, o usuarios con rol SOPORTE/ADMINISTRACION.
        /// Valida que KmFinal > KmInicial si aplica.
        /// </summary>
        /// <param name="id">ID de la ejecución.</param>
        /// <param name="usuarioId">ID del usuario que actualiza.</param>
        /// <param name="updates">Campos a actualizar.</param>
        /// <exception cref="UnauthorizedAccessException">Si no tiene permisos para actualizar.</exception>
        /// <exception cref="KeyNotFoundException">Si la ejecución no existe.</exception>
        /// <exception cref="ArgumentException">Si las validaciones fallan.</exception>
        public async Task UpdateEjecucionAsync(int id, int usuarioId, EjecucionOrdenUpdateRequestDto updates)
        {
            _logger.LogInformation("Actualizando ejecución {EjecucionId} por usuario {UsuarioId}", id, usuarioId);

            // ✅ Repository Pattern: obtener ejecución para actualización
            var ejecucion = await _ejecucionRepository.GetByIdAsync(id);
            if (ejecucion == null)
            {
                _logger.LogWarning("Ejecución {EjecucionId} no encontrada", id);
                throw new KeyNotFoundException($"Ejecución con ID {id} no encontrada.");
            }

            // Obtener el usuario que está actualizando para verificar su rol
            var usuarioActual = await _usuarioAuthService.ObtenerUsuarioPorIdAsync(usuarioId);
            if (usuarioActual == null)
            {
                _logger.LogWarning("Usuario {UsuarioId} no encontrado al actualizar ejecución {EjecucionId}", usuarioId, id);
                throw new UnauthorizedAccessException("Usuario no encontrado.");
            }

            // Validar permisos: Solo el técnico asignado, o usuarios con rol SOPORTE/ADMINISTRACION pueden actualizar
            bool puedeActualizar = ejecucion.TecnicoId == usuarioId || // Técnico asignado
                                   usuarioActual.Rol == RolUsuario.SOPORTE.ToString() || // Rol SOPORTE
                                   usuarioActual.Rol == RolUsuario.ADMINISTRACION.ToString() || // Rol ADMINISTRACION
                                   usuarioActual.Rol == "ADMINISTRADOR"; // Rol ADMINISTRADOR (compatibilidad)

            if (!puedeActualizar)
            {
                _logger.LogWarning("Usuario {UsuarioId} (rol: {Rol}) intentó actualizar ejecución {EjecucionId} asignada a {TecnicoId}",
                    usuarioId, usuarioActual.Rol, id, ejecucion.TecnicoId);
                throw new UnauthorizedAccessException("Solo el técnico asignado o usuarios con rol SOPORTE/ADMINISTRACION/ADMINISTRADOR pueden actualizar la ejecución.");
            }

            // Validación: Si ya está finalizada, no permitir cambios
            if (ejecucion.HrFin.HasValue && !updates.Comentarios?.Contains("[CORRECCIÓN]") == true)
            {
                _logger.LogWarning("Intento de modificar ejecución {EjecucionId} ya finalizada", id);
                throw new ArgumentException("La ejecución ya está finalizada. Use comentarios con [CORRECCIÓN] para modificar.");
            }

            // Aplicar updates con validaciones
            if (updates.HrFin.HasValue)
            {
                // Validar que HrFin > HrInicio
                if (ejecucion.HrInicio.HasValue && updates.HrFin.Value < ejecucion.HrInicio.Value)
                {
                    _logger.LogWarning("HrFin {HrFin} anterior a HrInicio {HrInicio}", updates.HrFin, ejecucion.HrInicio);
                    throw new ArgumentException("La hora de fin no puede ser anterior a la hora de inicio.");
                }
                ejecucion.HrFin = updates.HrFin;
            }

            if (updates.KmFinal.HasValue)
            {
                // Solo aplicable para ejecuciones de CAMPO
                if (ejecucion.TipoEjecucion != TipoEjecucion.CAMPO)
                {
                    _logger.LogWarning("Intento de actualizar KmFinal en ejecución {TipoEjecucion}", ejecucion.TipoEjecucion);
                    throw new ArgumentException("El kilometraje final solo aplica para ejecuciones de tipo CAMPO.");
                }

                // Validar que KmFinal >= KmInicial
                if (ejecucion.KmInicial.HasValue && updates.KmFinal.Value < ejecucion.KmInicial.Value)
                {
                    _logger.LogWarning("KmFinal {KmFinal} menor que KmInicial {KmInicial}", updates.KmFinal, ejecucion.KmInicial);
                    throw new ArgumentException($"El kilometraje final ({updates.KmFinal}) no puede ser menor que el inicial ({ejecucion.KmInicial}).");
                }

                ejecucion.KmFinal = updates.KmFinal;
            }

            if (!string.IsNullOrWhiteSpace(updates.Comentarios))
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                var nuevoComentario = $"[{timestamp}] {updates.Comentarios}";
                
                ejecucion.Comentarios = string.IsNullOrEmpty(ejecucion.Comentarios)
                    ? nuevoComentario
                    : $"{ejecucion.Comentarios}\n{nuevoComentario}";
            }

            // ✅ Repository Pattern: actualizar ejecución con validaciones
            await _ejecucionRepository.UpdateAsync(ejecucion);

            // Notificar finalización si se completó la ejecución
            if (updates.HrFin.HasValue && !ejecucion.HrFin.HasValue)
            {
                await _notificacionService.NotificarEjecucionFinalizadaAsync(id);
            }

            _logger.LogInformation("Ejecución {EjecucionId} actualizada exitosamente", id);
        }

        // =====================================================================================
        // MÉTODOS PARA EL NUEVO FLUJO DE EJECUCIONES
        // =====================================================================================

        /// <summary>
        /// Obtiene las tareas pendientes (órdenes sin ejecución activa).
        /// Solo accesible para usuarios con rol SOPORTE.
        /// </summary>
        /// <returns>Lista de tareas pendientes disponibles para tomar.</returns>
        public async Task<List<TareaPendienteDto>> ObtenerTareasPendientesAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo tareas pendientes para técnicos SOPORTE");

                // Obtener órdenes que están en estado CAPTURADA o ASIGNADA pero sin ejecución activa
                var ordenesPendientes = await _readContext.OrdenesTrabajo
                    .Where(o => o.Estado == "CAPTURADA" || o.Estado == "ASIGNADA")
                    .Where(o => !_readContext.EjecucionesOrden.Any(e => e.OrdenId == o.Id && !e.HrFin.HasValue))
                    .Include(o => o.CreadoPor)
                    .Include(o => o.AsignadaA)
                    .OrderByDescending(o => o.Prioridad)
                    .ThenBy(o => o.CreadoEn)
                    .ToListAsync();

                var tareas = new List<TareaPendienteDto>();

                foreach (var orden in ordenesPendientes)
                {
                    // Calcular tiempo de espera
                    var tiempoEspera = DateTime.UtcNow - (orden.CreadoEn ?? DateTime.UtcNow);
                    string tiempoEsperaFormateado;

                    if (tiempoEspera.TotalHours < 1)
                    {
                        tiempoEsperaFormateado = $"{(int)tiempoEspera.TotalMinutes}m";
                    }
                    else if (tiempoEspera.TotalDays < 1)
                    {
                        tiempoEsperaFormateado = $"{(int)tiempoEspera.TotalHours}h {(int)tiempoEspera.Minutes}m";
                    }
                    else
                    {
                        tiempoEsperaFormateado = $"{(int)tiempoEspera.TotalDays}d {(int)tiempoEspera.Hours}h";
                    }

                    // Determinar prioridad
                    string prioridad;
                    switch (orden.Prioridad)
                    {
                        case 1: prioridad = "BAJA"; break;
                        case 2: prioridad = "MEDIA"; break;
                        case 3: prioridad = "ALTA"; break;
                        case 4: prioridad = "URGENTE"; break;
                        case 5: prioridad = "URGENTE"; break;
                        default: prioridad = "MEDIA"; break;
                    }

                    var tarea = new TareaPendienteDto
                    {
                        OrdenId = orden.Id,
                        ClienteNombre = orden.NombreClienteCompleto,
                        VehiculoPlacas = null, // TODO: Obtener de cotización si existe
                        Descripcion = orden.Notas ?? $"Orden #{orden.Id} - {orden.TipoOrden ?? "Sin tipo"}",
                        Prioridad = prioridad,
                        FechaCreacion = orden.CreadoEn ?? DateTime.UtcNow,
                        TiempoEspera = tiempoEsperaFormateado,
                        AsignadaPor = orden.CreadoPor != null ? $"{orden.CreadoPor.Nombre} {orden.CreadoPor.Apellido}" : null,
                        TipoOrden = orden.TipoOrden,
                        Modalidad = orden.Modalidad,
                        Ubicacion = orden.UbicacionText
                    };

                    tareas.Add(tarea);
                }

                _logger.LogInformation("Se encontraron {Count} tareas pendientes", tareas.Count);
                return tareas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tareas pendientes");
                throw;
            }
        }

        /// <summary>
        /// Permite a un técnico SOPORTE tomar una tarea pendiente.
        /// Crea automáticamente una ejecución para la orden.
        /// </summary>
        /// <param name="request">Datos para tomar la tarea.</param>
        /// <param name="usuarioId">ID del técnico que toma la tarea.</param>
        /// <returns>Ejecución creada para la tarea tomada.</returns>
        public async Task<EjecucionOrdenResponseDto> TomarTareaAsync(TomarTareaRequestDto request, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Usuario {UsuarioId} intentando tomar tarea de orden {OrdenId}", usuarioId, request.OrdenId);

                // Verificar que el usuario tenga rol SOPORTE
                var usuario = await _usuarioAuthService.ObtenerUsuarioPorIdAsync(usuarioId);
                if (usuario == null || (usuario.Rol != RolUsuario.SOPORTE.ToString() && usuario.Rol != "ADMINISTRADOR"))
                {
                    throw new UnauthorizedAccessException("Solo usuarios con rol SOPORTE pueden tomar tareas.");
                }

                // Verificar que la orden existe y está disponible
                var orden = await _readContext.OrdenesTrabajo
                    .Where(o => o.Id == request.OrdenId)
                    .Where(o => o.Estado == "CAPTURADA" || o.Estado == "ASIGNADA")
                    .FirstOrDefaultAsync();

                if (orden == null)
                {
                    throw new ArgumentException("La orden no existe o no está disponible para tomar.", nameof(request.OrdenId));
                }

                // Verificar que no tenga ejecución activa
                var tieneEjecucionActiva = await _readContext.EjecucionesOrden
                    .AnyAsync(e => e.OrdenId == request.OrdenId && !e.HrFin.HasValue);

                if (tieneEjecucionActiva)
                {
                    throw new InvalidOperationException("La orden ya tiene una ejecución activa.");
                }

                // Validar campos según tipo de ejecución
                if (request.TipoEjecucion == TipoEjecucion.CAMPO.ToString())
                {
                    if (!request.VehiculoId.HasValue || !request.KmInicial.HasValue)
                    {
                        throw new ArgumentException("Para ejecuciones CAMPO se requiere vehículo y kilometraje inicial.");
                    }
                }

                // Crear DTO para la ejecución
                var createDto = new EjecucionOrdenCreateRequestDto
                {
                    OrdenId = request.OrdenId,
                    TecnicoId = usuarioId,
                    TipoEjecucion = Enum.Parse<TipoEjecucion>(request.TipoEjecucion),
                    HrInicio = DateTime.UtcNow,
                    VehiculoId = request.VehiculoId,
                    KmInicial = request.KmInicial,
                    Herramientas = request.Herramientas,
                    CodigoSesion = request.CodigoSesion,
                    ContrasenaSesion = request.ContrasenaSesion,
                    Comentarios = request.Comentarios
                };

                // Crear la ejecución
                var ejecucion = await CreateEjecucionAsync(createDto);

                // Actualizar estado de la orden a EN_CURSO
                var ordenEntity = await _writeContext.OrdenesTrabajo.FindAsync(request.OrdenId);
                if (ordenEntity != null)
                {
                    ordenEntity.Estado = "EN_CURSO";
                    ordenEntity.AsignadaAUserId = usuarioId;
                    ordenEntity.ActualizadoEn = DateTime.UtcNow;
                    await _writeContext.SaveChangesAsync();
                }

                _logger.LogInformation("Usuario {UsuarioId} tomó exitosamente la tarea de orden {OrdenId}, ejecución {EjecucionId} creada",
                    usuarioId, request.OrdenId, ejecucion.Id);

                // Notificar a otros técnicos que la tarea ya no está disponible
                await _notificacionService.NotificarTareaTomadaAsync(request.OrdenId, usuarioId);

                return ejecucion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al tomar tarea {OrdenId} por usuario {UsuarioId}", request.OrdenId, usuarioId);
                throw;
            }
        }

        /// <summary>
        /// Mapea una entidad EjecucionOrden a DTO de respuesta.
        /// Calcula campos derivados como DuracionMinutos, KmRecorridos y EstadoEjecucion.
        /// </summary>
        private EjecucionOrdenResponseDto MapToResponseDto(EjecucionOrden ejecucion)
        {
            // Calcular duración si ambas fechas existen
            int? duracionMinutos = null;
            if (ejecucion.HrInicio.HasValue && ejecucion.HrFin.HasValue)
            {
                duracionMinutos = (int)(ejecucion.HrFin.Value - ejecucion.HrInicio.Value).TotalMinutes;
            }

            // Calcular kilómetros recorridos
            int? kmRecorridos = null;
            if (ejecucion.KmInicial.HasValue && ejecucion.KmFinal.HasValue)
            {
                kmRecorridos = ejecucion.KmFinal.Value - ejecucion.KmInicial.Value;
            }

            // Determinar estado
            string estadoEjecucion = ejecucion.HrFin.HasValue ? "FINALIZADA" : "EN_CURSO";

            return new EjecucionOrdenResponseDto
            {
                Id = ejecucion.Id,
                OrdenId = ejecucion.OrdenId,
                TipoEjecucion = ejecucion.TipoEjecucion,
                TecnicoId = ejecucion.TecnicoId,
                TecnicoNombre = ejecucion.Tecnico != null 
                    ? $"{ejecucion.Tecnico.Nombre ?? ""} {ejecucion.Tecnico.Apellido ?? ""}".Trim() 
                    : null,
                HrInicio = ejecucion.HrInicio,
                HrFin = ejecucion.HrFin,
                DuracionMinutos = duracionMinutos,
                EstadoEjecucion = estadoEjecucion,
                Comentarios = ejecucion.Comentarios,
                // Campos CAMPO
                VehiculoId = ejecucion.VehiculoId,
                VehiculoPlacas = ejecucion.Vehiculo?.Placas,
                KmInicial = ejecucion.KmInicial,
                KmFinal = ejecucion.KmFinal,
                KmRecorridos = kmRecorridos,
                // Campos REMOTO
                Herramientas = ejecucion.Herramientas,
                CodigoSesion = ejecucion.CodigoSesion,
                ContrasenaSesion = ejecucion.ContrasenaSesion
            };
        }
    }

    /// <summary>
    /// DTO para actualizar una ejecución.
    /// </summary>
    public class EjecucionOrdenUpdateRequestDto
    {
        public DateTime? HrFin { get; set; }
        public int? KmFinal { get; set; }
        public string? Comentarios { get; set; }
    }

    /// <summary>
    /// DTO para una tarea pendiente.
    /// </summary>
    public class TareaPendienteDto
    {
        public int OrdenId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? VehiculoPlacas { get; set; }
        public string Descripcion { get; set; } = null!;
        public string Prioridad { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
        public string TiempoEspera { get; set; } = null!;
        public string? AsignadaPor { get; set; }
        public string? TipoOrden { get; set; }
        public string? Modalidad { get; set; }
        public string? Ubicacion { get; set; }
    }

    /// <summary>
    /// DTO para tomar una tarea pendiente.
    /// </summary>
    public class TomarTareaRequestDto
    {
        public int OrdenId { get; set; }
        public string TipoEjecucion { get; set; } = null!;
        public int? VehiculoId { get; set; }
        public int? KmInicial { get; set; }
        public string? Herramientas { get; set; }
        public string? CodigoSesion { get; set; }
        public string? ContrasenaSesion { get; set; }
        public string? Comentarios { get; set; }
    }
}