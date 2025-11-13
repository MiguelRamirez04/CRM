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
                    .OrderByDescending(c => c.Fecha) // Ordenado por la nueva fecha del documento
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

        // /// <summary>
        // /// Obtiene cotizaciones por el ID del documento de origen (ej. Orden de Trabajo).
        // /// </summary>
        // public async Task<IEnumerable<Cotizacion>> GetOrdenServicio(int IDConcepto)
        // {
        //     try
        //     {
        //         var resultado = await _readContext.Cotizaciones
        //             .AsNoTracking()
        //             .Where(c => c.ConceptoDocumentoId == IDConcepto) // Adaptado para usar DocumentoOrigenId
        //             .OrderByDescending(c => c.Fecha)
        //             .ToListAsync();

        //         _logger.LogDebug("Se obtuvo la Orden de Servicio con ID {resultado.id}", resultado.Count, IDConcepto);
        //         return resultado;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error al obtener cotizaciones por DocumentoOrigenId {OrdenId}", ordenId);
        //         throw;
        //     }
        // }

        /// <summary>
        /// Obtiene cotizaciones filtrando por un estado basado en las nuevas banderas.
        /// "CANCELADO": c.Cancelado == 1
        /// "AFECTADO": c.Afectado == 1
        /// "NUEVA" (o cualquier otro): c.Cancelado == 0 y c.Afectado == 0
        /// </summary>
        public async Task<IEnumerable<Cotizacion>> GetByEstadoAsync(string estado)
        {
            try
            {
                IQueryable<Cotizacion> query = _readContext.Cotizaciones.AsNoTracking();

                // Adaptación de la lógica de estado a las nuevas banderas
                switch (estado.ToUpper())
                {
                    case "CANCELADO":
                        query = query.Where(c => c.Cancelado == 1);
                        break;
                    case "AFECTADO":
                        query = query.Where(c => c.Afectado == 1);
                        break;
                    default: // Asumimos que cualquier otro estado (como "NUEVA") significa no cancelado y no afectado.
                        query = query.Where(c => c.Cancelado == 0 && c.Afectado == 0);
                        break;
                }

                var cotizaciones = await query.OrderByDescending(c => c.Fecha).ToListAsync();

                _logger.LogDebug("Obtenidas {Count} cotizaciones con estado {Estado}", cotizaciones.Count, estado);
                return cotizaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones por estado adaptado {Estado}", estado);
                throw;
            }
        }

        /// <summary>
        /// Obtiene cotizaciones buscando en la Razón Social del cliente.
        /// </summary>
        public async Task<IEnumerable<Cotizacion>> GetByClienteAsync(string cliente)
        {
            try
            {
                var cotizaciones = await _readContext.Cotizaciones
                    .AsNoTracking()
                    .Where(c => c.RazonSocial != null && c.RazonSocial.Contains(cliente)) // Adaptado para usar RazonSocial
                    .OrderByDescending(c => c.Fecha)
                    .ToListAsync();

                _logger.LogDebug("Obtenidas {Count} cotizaciones para cliente (Razón Social) {Cliente}", cotizaciones.Count, cliente);
                return cotizaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones por cliente (Razón Social) {Cliente}", cliente);
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

        public async Task<IEnumerable<Cotizacion>> GetByFechaCreadoAsync(DateTime fecha)
        {
            try
            {
                var fechaInicio = fecha.Date; // 00:00:00
                var fechaFin = fechaInicio.AddDays(1); // 23:59:59

                var cotizaciones = await _readContext.Cotizaciones
                    .AsNoTracking()
                    .Where(c => c.CreadoEn >= fechaInicio && c.CreadoEn < fechaFin)
                    .OrderBy(c => c.CreadoEn)
                    .ToListAsync();

                _logger.LogDebug("Obtenidas {Count} cotizaciones para fecha {Fecha}", cotizaciones.Count, fecha.ToShortDateString());
                return cotizaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones por fecha {Fecha}", fecha);
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
    }
}