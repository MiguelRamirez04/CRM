using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Shared;
using back_cabs.CRM.models.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.repositories.Shared
{
    public class UsoVehiculoRepository : IUsoVehiculoRepository
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<UsoVehiculoRepository> _logger;

        public UsoVehiculoRepository(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            ILogger<UsoVehiculoRepository> logger)
        {
            _writeContext = writeContext;
            _readContext = readContext;
            _logger = logger;
        }

        public async Task<UsoVehiculo> CreateAsync(UsoVehiculo usoVehiculo)
        {
            try
            {
                usoVehiculo.FechaCreacion = DateTime.UtcNow;
                _writeContext.Set<UsoVehiculo>().Add(usoVehiculo);
                await _writeContext.SaveChangesAsync();
                return usoVehiculo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear registro de uso de vehículo");
                throw;
            }
        }

        public async Task<UsoVehiculo> UpdateAsync(UsoVehiculo usoVehiculo)
        {
            try
            {
                usoVehiculo.FechaActualizacion = DateTime.UtcNow;
                _writeContext.Set<UsoVehiculo>().Update(usoVehiculo);
                await _writeContext.SaveChangesAsync();
                return usoVehiculo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar registro de uso de vehículo");
                throw;
            }
        }

        public async Task<UsoVehiculo?> GetUsoActivoPorVehiculoAsync(int vehiculoId)
        {
            return await _readContext.Set<UsoVehiculo>()
                .Where(u => u.VehiculoId == vehiculoId && u.Estado == "EN_USO")
                .OrderByDescending(u => u.FechaCreacion)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UsoVehiculo>> GetHistorialPorVehiculoAsync(int vehiculoId)
        {
            return await _readContext.Set<UsoVehiculo>()
                .Include(u => u.Usuario)
                .Where(u => u.VehiculoId == vehiculoId)
                .OrderByDescending(u => u.FechaInicio)
                .ToListAsync();
        }

        public async Task<UsoVehiculo?> GetByIdAsync(int id)
        {
            return await _readContext.Set<UsoVehiculo>()
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
