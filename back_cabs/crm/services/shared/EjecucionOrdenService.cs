using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.enums;
using back_cabs.CRM.models.Soporte;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.services.shared
{
    /// <summary>
    /// Servicio para gestionar ejecuciones de órdenes de trabajo.
    /// Maneja operaciones CRUD y lógica de delegación entre técnicos.
    /// </summary>
    public class EjecucionOrdenService
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<EjecucionOrdenService> _logger;

        public EjecucionOrdenService(WriteContext writeContext, ReadOnlyContext readContext, ILogger<EjecucionOrdenService> logger)
        {
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea una nueva ejecución de orden de trabajo.
        /// Valida que el técnico tenga rol SOPORTE y que la orden exista.
        /// </summary>
        /// <param name="dto">Datos de la ejecución a crear.</param>
        /// <returns>DTO de respuesta con la ejecución creada.</returns>
        /// <exception cref="ArgumentException">Si el técnico no tiene rol SOPORTE o la orden no existe.</exception>
        public async Task<EjecucionOrdenResponseDto> CreateEjecucionAsync(EjecucionOrdenCreateRequestDto dto)
        {
            _logger.LogInformation("Creando nueva ejecución para orden {OrdenId} con técnico {TecnicoId}", dto.OrdenId, dto.TecnicoId);

            // Validar que la orden exista
            var orden = await _readContext.OrdenesTrabajo.FindAsync(dto.OrdenId);
            if (orden == null)
                throw new ArgumentException("La orden de trabajo especificada no existe.", nameof(dto.OrdenId));

            // Validar que el técnico exista y tenga rol SOPORTE
            var tecnico = await _readContext.UsuariosAuth.FindAsync(dto.TecnicoId);
            if (tecnico == null || tecnico.Rol != RolUsuario.SOPORTE.ToString())
                throw new ArgumentException("El técnico debe existir y tener rol SOPORTE.", nameof(dto.TecnicoId));

            // Crear la entidad
            var ejecucion = new EjecucionOrden
            {
                OrdenId = dto.OrdenId,
                TipoEjecucion = dto.TipoEjecucion,
                TecnicoId = dto.TecnicoId,
                HrInicio = dto.HrInicio,
                Comentarios = dto.Comentarios,
                VehiculoId = dto.VehiculoId,
                KmInicial = dto.KmInicial,
                Herramientas = dto.Herramientas,
                CodigoSesion = dto.CodigoSesion,
                ContrasenaSesion = dto.ContrasenaSesion
            };

            using var transaction = await _writeContext.Database.BeginTransactionAsync();
            try
            {
                _writeContext.EjecucionesOrden.Add(ejecucion);
                await _writeContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Ejecución creada exitosamente con ID {EjecucionId}", ejecucion.Id);

                // Mapear a response
                return MapToResponseDto(ejecucion);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear ejecución para orden {OrdenId}", dto.OrdenId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene una ejecución específica por ID.
        /// </summary>
        /// <param name="id">ID de la ejecución.</param>
        /// <returns>DTO de respuesta o null si no existe.</returns>
        public async Task<EjecucionOrdenResponseDto?> GetEjecucionByIdAsync(int id)
        {
            _logger.LogInformation("Consultando ejecución con ID {EjecucionId}", id);

            var ejecucion = await _readContext.EjecucionesOrden
                .Include(e => e.Orden)
                .Include(e => e.Tecnico)
                .Include(e => e.Vehiculo)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

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

            var query = _readContext.EjecucionesOrden
                .Include(e => e.Orden)
                .Include(e => e.Tecnico)
                .Include(e => e.Vehiculo)
                .AsNoTracking()
                .AsQueryable();

            if (ordenId.HasValue)
                query = query.Where(e => e.OrdenId == ordenId.Value);
            if (tecnicoId.HasValue)
                query = query.Where(e => e.TecnicoId == tecnicoId.Value);
            if (tipoEjecucion.HasValue)
                query = query.Where(e => e.TipoEjecucion == tipoEjecucion.Value);
            if (fechaDesde.HasValue)
                query = query.Where(e => e.HrInicio >= fechaDesde.Value);
            if (fechaHasta.HasValue)
                query = query.Where(e => e.HrInicio <= fechaHasta.Value);

            var ejecuciones = await query.ToListAsync();
            var result = new List<EjecucionOrdenResponseDto>();
            foreach (var ejecucion in ejecuciones)
            {
                result.Add(MapToResponseDto(ejecucion));
            }

            _logger.LogInformation("Se encontraron {Count} ejecuciones", result.Count);
            return result;
        }

        /// <summary>
        /// Delega una ejecución a otro técnico (cambia tecnico_id).
        /// Solo usuarios con rol SOPORTE pueden delegar, y el nuevo técnico debe tener rol SOPORTE.
        /// Registra el cambio en comentarios.
        /// </summary>
        /// <param name="ejecucionId">ID de la ejecución.</param>
        /// <param name="nuevoTecnicoId">ID del nuevo técnico.</param>
        /// <param name="usuarioActualId">ID del usuario que delega.</param>
        /// <param name="motivo">Motivo de la delegación.</param>
        /// <exception cref="UnauthorizedAccessException">Si el usuario no tiene rol SOPORTE.</exception>
        /// <exception cref="ArgumentException">Si el nuevo técnico no es válido.</exception>
        /// <exception cref="KeyNotFoundException">Si la ejecución no existe.</exception>
        public async Task DelegateEjecucionAsync(int ejecucionId, int nuevoTecnicoId, int usuarioActualId, string motivo)
        {
            _logger.LogInformation("Delegando ejecución {EjecucionId} de usuario {UsuarioActualId} a técnico {NuevoTecnicoId}", ejecucionId, usuarioActualId, nuevoTecnicoId);

            // Validar usuario actual
            var usuarioActual = await _readContext.UsuariosAuth.FindAsync(usuarioActualId);
            if (usuarioActual == null || usuarioActual.Rol != RolUsuario.SOPORTE.ToString())
                throw new UnauthorizedAccessException("Solo usuarios con rol SOPORTE pueden delegar ejecuciones.");

            // Validar nuevo técnico
            var nuevoTecnico = await _readContext.UsuariosAuth.FindAsync(nuevoTecnicoId);
            if (nuevoTecnico == null || nuevoTecnico.Rol != RolUsuario.SOPORTE.ToString())
                throw new ArgumentException("El nuevo técnico debe existir y tener rol SOPORTE.", nameof(nuevoTecnicoId));

            // Obtener ejecución
            var ejecucion = await _writeContext.EjecucionesOrden.FindAsync(ejecucionId);
            if (ejecucion == null)
                throw new KeyNotFoundException($"Ejecución con ID {ejecucionId} no encontrada.");

            // Evitar delegación a sí mismo
            if (ejecucion.TecnicoId == nuevoTecnicoId)
                throw new ArgumentException("No se puede delegar a sí mismo.", nameof(nuevoTecnicoId));

            // Actualizar y registrar cambio
            var comentarioDelegacion = $"[DELEGACIÓN {DateTime.Now:yyyy-MM-dd HH:mm}] De {usuarioActual.Nombre} {usuarioActual.Apellido} a {nuevoTecnico.Nombre} {nuevoTecnico.Apellido}. Motivo: {motivo}";
            ejecucion.TecnicoId = nuevoTecnicoId;
            ejecucion.Comentarios = string.IsNullOrEmpty(ejecucion.Comentarios)
                ? comentarioDelegacion
                : $"{ejecucion.Comentarios}\n{comentarioDelegacion}";

            await _writeContext.SaveChangesAsync();

            _logger.LogInformation("Ejecución {EjecucionId} delegada exitosamente a técnico {NuevoTecnicoId}", ejecucionId, nuevoTecnicoId);
        }

        /// <summary>
        /// Actualiza campos de una ejecución (ej. finalizar con HrFin, KmFinal).
        /// Solo el técnico asignado puede actualizar.
        /// </summary>
        /// <param name="id">ID de la ejecución.</param>
        /// <param name="usuarioId">ID del usuario que actualiza.</param>
        /// <param name="updates">Campos a actualizar.</param>
        /// <exception cref="UnauthorizedAccessException">Si no es el técnico asignado.</exception>
        public async Task UpdateEjecucionAsync(int id, int usuarioId, EjecucionOrdenUpdateRequestDto updates)
        {
            _logger.LogInformation("Actualizando ejecución {EjecucionId} por usuario {UsuarioId}", id, usuarioId);

            var ejecucion = await _writeContext.EjecucionesOrden.FindAsync(id);
            if (ejecucion == null)
                throw new KeyNotFoundException($"Ejecución con ID {id} no encontrada.");

            // Validar que sea el técnico asignado
            if (ejecucion.TecnicoId != usuarioId)
                throw new UnauthorizedAccessException("Solo el técnico asignado puede actualizar la ejecución.");

            // Aplicar updates
            if (updates.HrFin.HasValue) ejecucion.HrFin = updates.HrFin;
            if (updates.KmFinal.HasValue) ejecucion.KmFinal = updates.KmFinal;
            if (!string.IsNullOrEmpty(updates.Comentarios))
                ejecucion.Comentarios = string.IsNullOrEmpty(ejecucion.Comentarios)
                    ? updates.Comentarios
                    : $"{ejecucion.Comentarios}\n{updates.Comentarios}";

            await _writeContext.SaveChangesAsync();

            _logger.LogInformation("Ejecución {EjecucionId} actualizada exitosamente", id);
        }

        /// <summary>
        /// Mapea una entidad EjecucionOrden a DTO de respuesta.
        /// </summary>
        private EjecucionOrdenResponseDto MapToResponseDto(EjecucionOrden ejecucion)
        {
            return new EjecucionOrdenResponseDto
            {
                Id = ejecucion.Id,
                OrdenId = ejecucion.OrdenId,
                TipoEjecucion = ejecucion.TipoEjecucion,
                TecnicoId = ejecucion.TecnicoId,
                TecnicoNombre = ejecucion.Tecnico?.Nombre + " " + ejecucion.Tecnico?.Apellido,
                HrInicio = ejecucion.HrInicio,
                HrFin = ejecucion.HrFin,
                Comentarios = ejecucion.Comentarios,
                VehiculoId = ejecucion.VehiculoId,
                VehiculoPlacas = ejecucion.Vehiculo?.Placas,
                KmInicial = ejecucion.KmInicial,
                KmFinal = ejecucion.KmFinal,
                Herramientas = ejecucion.Herramientas,
                CodigoSesion = ejecucion.CodigoSesion
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
}