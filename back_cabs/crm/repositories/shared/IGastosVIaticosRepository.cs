using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces;
using back_cabs.CRM.models.Shared;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.Repositories
{
    /// <summary>
    /// Repositorio para gestionar gastos de viáticos
    /// </summary>
    public class GastoViaticoRepository : IGastoViaticoRepository
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;

        public GastoViaticoRepository(WriteContext writeContext, ReadOnlyContext readContext)
        {
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
        }

        public async Task<GastoViatico> CreateViaticoAsync(GastoViatico viatico)
        {
            // Si ordenId es 0 o negativo, convertirlo a null para evitar conflictos con FK
            if (viatico.OrdenId.HasValue && viatico.OrdenId.Value <= 0)
            {
                viatico.OrdenId = null;
            }

            _writeContext.GastosViaticos.Add(viatico);
            await _writeContext.SaveChangesAsync();
            return viatico;
        }

        public async Task<GastoViatico?> GetViaticoByIdReadOnlyAsync(int id)
        {
            return await _readContext.GastosViaticos
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        /// <summary>
        /// Obtiene un viático por ID incluyendo el vehículo relacionado (si existe)
        /// </summary>
        public async Task<GastoViatico?> GetViaticoConVehiculoAsync(int id)
        {
            return await _readContext.GastosViaticos
                .AsNoTracking()
                .Include(v => v.Vehiculo) // ✅ Cargar vehículo para evitar N+1
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<GastoViatico?> GetViaticoByIdForUpdateAsync(int id)
        {
            return await _writeContext.GastosViaticos.FindAsync(id);
        }

        public async Task<(List<GastoViatico> Items, int TotalCount)> GetViaticosFilteredAsync(
            int? ordenId, DateTime? fechaDesde, DateTime? fechaHasta, int pageNumber, int pageSize)
        {
            var query = _readContext.GastosViaticos
                .AsNoTracking()
                .AsQueryable();

            if (ordenId.HasValue)
                query = query.Where(v => v.OrdenId == ordenId.Value);
            if (fechaDesde.HasValue)
                query = query.Where(v => v.Fecha >= fechaDesde.Value);
            if (fechaHasta.HasValue)
                query = query.Where(v => v.Fecha <= fechaHasta.Value);

            var totalCount = await query.CountAsync();

            var viaticos = await query
                .OrderByDescending(v => v.Fecha)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (viaticos, totalCount);
        }

        public async Task SaveChangesAsync()
        {
            await _writeContext.SaveChangesAsync();
        }
    }
}