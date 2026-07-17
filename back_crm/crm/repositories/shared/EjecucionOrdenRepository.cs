using back_cabs.CRM.contexts;
using back_cabs.CRM.enums;
using back_cabs.CRM.Interfaces.Recepcion;
using back_cabs.CRM.models.Soporte;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.repositories.Recepcion
{
    /// <summary>
    /// Implementación de IEjecucionOrdenRepository usando Entity Framework Core.
    /// Encapsula todas las operaciones de BD para Ejecuciones de Orden.
    /// </summary>
    public class EjecucionOrdenRepository : IEjecucionOrdenRepository
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<EjecucionOrdenRepository> _logger;

        public EjecucionOrdenRepository(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            ILogger<EjecucionOrdenRepository> logger)
        {
            _writeContext = writeContext;
            _readContext = readContext;
            _logger = logger;
        }

        // 📖 IMPLEMENTACIÓN DE LECTURAS

        public async Task<IEnumerable<EjecucionOrden>> GetAllAsync(
            int? ordenId = null,
            int? tecnicoId = null,
            TipoEjecucion? tipo = null,
            DateTime? desde = null,
            DateTime? hasta = null)
        {
            try
            {
                var query = _readContext.EjecucionesOrden
                    .AsNoTracking()
                    .Include(e => e.Orden)
                    .Include(e => e.Tecnico)
                    .Include(e => e.Vehiculo)
                    .AsQueryable();

                // Aplicar filtros opcionales
                if (ordenId.HasValue)
                    query = query.Where(e => e.OrdenId == ordenId.Value);

                if (tecnicoId.HasValue)
                    query = query.Where(e => e.TecnicoId == tecnicoId.Value);

                if (tipo.HasValue)
                    query = query.Where(e => e.TipoEjecucion == tipo.Value);

                if (desde.HasValue)
                    query = query.Where(e => e.HrInicio >= desde.Value);

                if (hasta.HasValue)
                    query = query.Where(e => e.HrInicio <= hasta.Value);

                var ejecuciones = await query
                    .OrderByDescending(e => e.HrInicio)
                    .ToListAsync();

                _logger.LogDebug("Obtenidas {Count} ejecuciones con filtros", ejecuciones.Count);
                return ejecuciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ejecuciones con filtros");
                throw;
            }
        }

        public async Task<EjecucionOrden?> GetByIdAsync(int id)
        {
            try
            {
                var ejecucion = await _readContext.EjecucionesOrden
                    .AsNoTracking()
                    .Include(e => e.Orden)
                    .Include(e => e.Tecnico)
                    .Include(e => e.Vehiculo)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (ejecucion == null)
                {
                    _logger.LogDebug("Ejecución con ID {Id} no encontrada", id);
                }

                return ejecucion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ejecución con ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<EjecucionOrden>> GetByOrdenIdAsync(int ordenId)
        {
            try
            {
                var ejecuciones = await _readContext.EjecucionesOrden
                    .AsNoTracking()
                    .Include(e => e.Tecnico)
                    .Include(e => e.Vehiculo)
                    .Where(e => e.OrdenId == ordenId)
                    .OrderByDescending(e => e.HrInicio)
                    .ToListAsync();

                _logger.LogDebug("Encontradas {Count} ejecuciones para orden {OrdenId}",
                    ejecuciones.Count, ordenId);
                return ejecuciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ejecuciones para orden {OrdenId}", ordenId);
                throw;
            }
        }

        public async Task<IEnumerable<EjecucionOrden>> GetByTecnicoIdAsync(int tecnicoId)
        {
            try
            {
                var ejecuciones = await _readContext.EjecucionesOrden
                    .AsNoTracking()
                    .Include(e => e.Orden)
                    .Include(e => e.Vehiculo)
                    .Where(e => e.TecnicoId == tecnicoId)
                    .OrderByDescending(e => e.HrInicio)
                    .ToListAsync();

                _logger.LogDebug("Encontradas {Count} ejecuciones para técnico {TecnicoId}",
                    ejecuciones.Count, tecnicoId);
                return ejecuciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ejecuciones para técnico {TecnicoId}", tecnicoId);
                throw;
            }
        }

        public async Task<IEnumerable<EjecucionOrden>> GetEjecucionesActivasAsync()
        {
            try
            {
                var ejecuciones = await _readContext.EjecucionesOrden
                    .AsNoTracking()
                    .Include(e => e.Orden)
                    .Include(e => e.Tecnico)
                    .Include(e => e.Vehiculo)
                    .Where(e => e.HrFin == null)
                    .OrderBy(e => e.HrInicio)
                    .ToListAsync();

                _logger.LogDebug("Encontradas {Count} ejecuciones activas", ejecuciones.Count);
                return ejecuciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ejecuciones activas");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var exists = await _readContext.EjecucionesOrden
                    .AnyAsync(e => e.Id == id);

                _logger.LogDebug("Ejecución ID {Id} existe: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de ejecución ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> TecnicoTieneEjecucionesActivasAsync(int tecnicoId)
        {
            try
            {
                var tieneActivas = await _readContext.EjecucionesOrden
                    .AnyAsync(e => e.TecnicoId == tecnicoId && e.HrFin == null);

                _logger.LogDebug("Técnico {TecnicoId} tiene ejecuciones activas: {TieneActivas}",
                    tecnicoId, tieneActivas);
                return tieneActivas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar ejecuciones activas para técnico {TecnicoId}", tecnicoId);
                throw;
            }
        }

        public async Task<bool> OrdenTieneEjecucionesActivasAsync(int ordenId)
        {
            try
            {
                var tieneActivas = await _readContext.EjecucionesOrden
                    .AnyAsync(e => e.OrdenId == ordenId && e.HrFin == null);

                _logger.LogDebug("Orden {OrdenId} tiene ejecuciones activas: {TieneActivas}",
                    ordenId, tieneActivas);
                return tieneActivas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar ejecuciones activas para orden {OrdenId}", ordenId);
                throw;
            }
        }

        // ✏️ IMPLEMENTACIÓN DE ESCRITURAS

        public async Task<EjecucionOrden> CreateAsync(EjecucionOrden ejecucion)
        {
            try
            {
                _writeContext.EjecucionesOrden.Add(ejecucion);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Ejecución creada con ID {Id} para orden {OrdenId}",
                    ejecucion.Id, ejecucion.OrdenId);
                return ejecucion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ejecución para orden {OrdenId}", ejecucion.OrdenId);
                throw;
            }
        }

        public async Task<EjecucionOrden> UpdateAsync(EjecucionOrden ejecucion)
        {
            try
            {
                _writeContext.EjecucionesOrden.Update(ejecucion);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Ejecución actualizada con ID {Id}", ejecucion.Id);
                return ejecucion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar ejecución con ID {Id}", ejecucion.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var ejecucion = await _writeContext.EjecucionesOrden.FindAsync(id);
                if (ejecucion == null)
                {
                    _logger.LogDebug("Ejecución ID {Id} no encontrada para eliminación", id);
                    return false;
                }

                _writeContext.EjecucionesOrden.Remove(ejecucion);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Ejecución eliminada con ID {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar ejecución con ID {Id}", id);
                throw;
            }
        }

        public async Task<EjecucionOrden> DelegateAsync(int ejecucionId, int nuevoTecnicoId, int usuarioActualId)
        {
            try
            {
                var ejecucion = await _writeContext.EjecucionesOrden
                    .Include(e => e.Tecnico)
                    .FirstOrDefaultAsync(e => e.Id == ejecucionId);

                if (ejecucion == null)
                {
                    throw new KeyNotFoundException($"Ejecución con ID {ejecucionId} no encontrada");
                }

                // Verificar que no se delegue a sí mismo
                if (ejecucion.TecnicoId == nuevoTecnicoId)
                {
                    throw new InvalidOperationException("No se puede delegar a la misma persona");
                }

                var tecnicoAnteriorId = ejecucion.TecnicoId;
                ejecucion.TecnicoId = nuevoTecnicoId;

                // Agregar comentario de delegación
                var comentarioActual = ejecucion.Comentarios ?? "";
                var comentarioDelegacion = $"[DELEGACIÓN {DateTime.UtcNow:yyyy-MM-dd HH:mm}] " +
                    $"Delegada de técnico {tecnicoAnteriorId} a {nuevoTecnicoId} por usuario {usuarioActualId}";
                ejecucion.Comentarios = comentarioActual + Environment.NewLine + comentarioDelegacion;

                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Ejecución {EjecucionId} delegada de técnico {Anterior} a {Nuevo}",
                    ejecucionId, tecnicoAnteriorId, nuevoTecnicoId);

                return ejecucion;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Error al delegar ejecución {EjecucionId}", ejecucionId);
                throw;
            }
        }
    }
}