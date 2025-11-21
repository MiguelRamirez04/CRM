using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Recepcion;
using back_cabs.CRM.models.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.repositories.Recepcion
{
    /// <summary>
    /// Implementación de ICotizacionRepository usando Entity Framework Core.
    /// Encapsula todas las operaciones de BD para Cotizaciones.
    /// </summary>
    public class CotizacionRepository : ICotizacionRepository
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<CotizacionRepository> _logger;

        public CotizacionRepository(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            ILogger<CotizacionRepository> logger)
        {
            _writeContext = writeContext;
            _readContext = readContext;
            _logger = logger;
        }

        // 📖 IMPLEMENTACIÓN DE LECTURAS

        public async Task<IEnumerable<Cotizacion>> GetAllAsync()
        {
            try
            {
                var cotizaciones = await _readContext.Cotizaciones
                    .AsNoTracking()
                    .OrderByDescending(c => c.CreadoEn)
                    .ToListAsync();

                _logger.LogDebug("Obtenidas {Count} cotizaciones", cotizaciones.Count);
                return cotizaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las cotizaciones");
                throw;
            }
        }

        public async Task<Cotizacion?> GetByIdAsync(int id)
        {
            try
            {
                var cotizacion = await _readContext.Cotizaciones
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cotizacion != null)
                {
                    _logger.LogDebug("Cotización {Id} encontrada", id);
                }
                else
                {
                    _logger.LogDebug("Cotización {Id} no encontrada", id);
                }

                return cotizacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotización por ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Cotizacion>> GetByOrdenIdAsync(int ordenId)
        {
            try
            {
                var cotizaciones = await _readContext.Cotizaciones
                    .AsNoTracking()
                    .Where(c => c.OrdenId == ordenId)
                    .OrderByDescending(c => c.CreadoEn)
                    .ToListAsync();

                _logger.LogDebug("Obtenidas {Count} cotizaciones para OrdenId {OrdenId}", cotizaciones.Count, ordenId);
                return cotizaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones por OrdenId {OrdenId}", ordenId);
                throw;
            }
        }

        public async Task<IEnumerable<Cotizacion>> GetByEstadoAsync(string estado)
        {
            try
            {
                var cotizaciones = await _readContext.Cotizaciones
                    .AsNoTracking()
                    .Where(c => c.Estado == estado)
                    .OrderByDescending(c => c.CreadoEn)
                    .ToListAsync();

                _logger.LogDebug("Obtenidas {Count} cotizaciones con estado {Estado}", cotizaciones.Count, estado);
                return cotizaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones por estado {Estado}", estado);
                throw;
            }
        }

        public async Task<IEnumerable<Cotizacion>> GetByClienteAsync(string cliente)
        {
            try
            {
                var cotizaciones = await _readContext.Cotizaciones
                    .AsNoTracking()
                    .Where(c => c.Cliente != null && c.Cliente.Contains(cliente))
                    .OrderByDescending(c => c.CreadoEn)
                    .ToListAsync();

                _logger.LogDebug("Obtenidas {Count} cotizaciones para cliente {Cliente}", cotizaciones.Count, cliente);
                return cotizaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones por cliente {Cliente}", cliente);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var exists = await _readContext.Cotizaciones
                    .AnyAsync(c => c.Id == id);

                _logger.LogDebug("Cotización {Id} existe: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de cotización {Id}", id);
                throw;
            }
        }

        // ✏️ IMPLEMENTACIÓN DE ESCRITURAS

        public async Task<Cotizacion> CreateAsync(Cotizacion cotizacion)
        {
            try
            {
                _writeContext.Cotizaciones.Add(cotizacion);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Cotización creada con ID {Id}", cotizacion.Id);
                return cotizacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cotización");
                throw;
            }
        }

        public async Task<Cotizacion> UpdateAsync(Cotizacion cotizacion)
        {
            try
            {
                _writeContext.Cotizaciones.Update(cotizacion);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Cotización {Id} actualizada", cotizacion.Id);
                return cotizacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cotización {Id}", cotizacion.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var cotizacion = await _writeContext.Cotizaciones.FindAsync(id);
                if (cotizacion == null)
                {
                    _logger.LogWarning("Intento de eliminar cotización {Id} que no existe", id);
                    return false;
                }

                _writeContext.Cotizaciones.Remove(cotizacion);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Cotización {Id} eliminada", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cotización {Id}", id);
                throw;
            }
        }

        public Task<IEnumerable<Cotizacion>> GetByEstadoAsync(string campo, int valor)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Cotizacion>> GetByClienteIdAsync(int clienteId)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, bool>> ValidarLlavesForaneasAsync(int documentoDeId, int conceptoDocumentoId, int clienteProveedorId, int agenteId)
        {
            throw new NotImplementedException();
        }
    }
}