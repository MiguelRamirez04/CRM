using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Shared;
using back_cabs.CRM.models.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.repositories.Shared
{
    /// <summary>
    /// Implementación de IVehiculoRepository usando Entity Framework Core.
    /// Encapsula todas las operaciones de BD para Vehículos.
    /// </summary>
    public class VehiculoRepository : IVehiculoRepository
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<VehiculoRepository> _logger;

        public VehiculoRepository(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            ILogger<VehiculoRepository> logger)
        {
            _writeContext = writeContext;
            _readContext = readContext;
            _logger = logger;
        }

        // 📖 IMPLEMENTACIÓN DE LECTURAS

        public async Task<IEnumerable<Vehiculo>> GetAllAsync()
        {
            try
            {
                var vehiculos = await _readContext.Vehiculos
                    .AsNoTracking()
                    .OrderBy(v => v.Placas)
                    .ToListAsync();

                _logger.LogDebug("Obtenidos {Count} vehículos", vehiculos.Count);
                return vehiculos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los vehículos");
                throw;
            }
        }

        public async Task<Vehiculo?> GetByIdAsync(int id)
        {
            try
            {
                var vehiculo = await _readContext.Vehiculos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (vehiculo == null)
                {
                    _logger.LogDebug("Vehículo con ID {Id} no encontrado", id);
                }

                return vehiculo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículo con ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Vehiculo>> GetByTipoAsync(string tipoVehiculo)
        {
            try
            {
                var vehiculos = await _readContext.Vehiculos
                    .AsNoTracking()
                    .Where(v => v.TipoVehiculo == tipoVehiculo)
                    .OrderBy(v => v.Placas)
                    .ToListAsync();

                _logger.LogDebug("Encontrados {Count} vehículos de tipo {Tipo}",
                    vehiculos.Count, tipoVehiculo);
                return vehiculos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos por tipo {Tipo}", tipoVehiculo);
                throw;
            }
        }

        public async Task<IEnumerable<Vehiculo>> GetActivosAsync()
        {
            try
            {
                var vehiculos = await _readContext.Vehiculos
                    .AsNoTracking()
                    .Where(v => v.Activo)
                    .OrderBy(v => v.Placas)
                    .ToListAsync();

                _logger.LogDebug("Encontrados {Count} vehículos activos", vehiculos.Count);
                return vehiculos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos activos");
                throw;
            }
        }

        public async Task<Vehiculo?> GetByPlacasAsync(string placas)
        {
            try
            {
                var vehiculo = await _readContext.Vehiculos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Placas == placas);

                if (vehiculo == null)
                {
                    _logger.LogDebug("Vehículo con placas {Placas} no encontrado", placas);
                }

                return vehiculo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar vehículo con placas {Placas}", placas);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var exists = await _readContext.Vehiculos
                    .AnyAsync(v => v.Id == id);

                _logger.LogDebug("Vehículo ID {Id} existe: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de vehículo ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> PlacasExistAsync(string placas)
        {
            try
            {
                var exists = await _readContext.Vehiculos
                    .AnyAsync(v => v.Placas == placas);

                _logger.LogDebug("Placas {Placas} existen: {Exists}", placas, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de placas {Placas}", placas);
                throw;
            }
        }

        // ✏️ IMPLEMENTACIÓN DE ESCRITURAS

        public async Task<Vehiculo> CreateAsync(Vehiculo vehiculo)
        {
            try
            {
                _writeContext.Vehiculos.Add(vehiculo);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Vehículo creado con ID {Id} y placas {Placas}",
                    vehiculo.Id, vehiculo.Placas);
                return vehiculo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear vehículo con placas {Placas}", vehiculo.Placas);
                throw;
            }
        }

        public async Task<Vehiculo> UpdateAsync(Vehiculo vehiculo)
        {
            try
            {
                _writeContext.Vehiculos.Update(vehiculo);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Vehículo actualizado con ID {Id}", vehiculo.Id);
                return vehiculo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar vehículo con ID {Id}", vehiculo.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var vehiculo = await _writeContext.Vehiculos.FindAsync(id);
                if (vehiculo == null)
                {
                    _logger.LogDebug("Vehículo ID {Id} no encontrado para eliminación", id);
                    return false;
                }

                _writeContext.Vehiculos.Remove(vehiculo);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Vehículo eliminado con ID {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar vehículo con ID {Id}", id);
                throw;
            }
        }
    }
}