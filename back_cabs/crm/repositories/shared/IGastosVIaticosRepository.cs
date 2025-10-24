using back_cabs.CRM.contexts;
using back_cabs.CRM.enums;
using back_cabs.CRM.Interfaces;
using back_cabs.CRM.models.Shared;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.Repositories
{
    // Esta es la NUEVA clase de Repositorio
    public class GastoViaticoRepository : IGastoViaticoRepository
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;

        public GastoViaticoRepository(WriteContext writeContext, ReadOnlyContext readContext)
        {
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
        }

        public async Task<bool> UsuarioExistsAsync(int usuarioId)
        {
            return await _readContext.UsuariosAuth.AnyAsync(u => u.Id == usuarioId);
        }

        public async Task<GastoViatico> CreateViaticoAsync(GastoViatico viatico)
        {
            // La lógica de transacción se mueve aquí
            using var transaction = await _writeContext.Database.BeginTransactionAsync();
            try
            {
                _writeContext.GastosViaticosNuevos.Add(viatico);
                await _writeContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return viatico;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Relanza la excepción para que el servicio la maneje
            }
        }
        
        public async Task<GastoViatico?> GetViaticoByIdReadOnlyAsync(int id)
        {
            return await _readContext.GastosViaticosNuevos
                .Include(v => v.Detalles)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<GastoViatico?> GetViaticoByIdForUpdateAsync(int id)
        {
            // Usamos FindAsync en el WriteContext para que EF rastree la entidad
            return await _writeContext.GastosViaticosNuevos.FindAsync(id);
        }

        public async Task<(List<GastoViatico> Items, int TotalCount)> GetViaticosFilteredAsync(
            int? usuarioId, TipoViatico? tipoViatico, EstadoGasto? estadoGasto,
            DateTime? fechaDesde, DateTime? fechaHasta, int pageNumber, int pageSize)
        {
            var query = _readContext.GastosViaticosNuevos
                .Include(v => v.Detalles)
                .AsNoTracking()
                .AsQueryable();

            if (tipoViatico.HasValue)
                query = query.Where(v => v.TipoViatico == tipoViatico.Value);
            if (usuarioId.HasValue)
                query = query.Where(v => v.UsuarioId == usuarioId.Value);
            if (estadoGasto.HasValue)
                query = query.Where(v => v.EstadoGasto == estadoGasto.Value);
            if (fechaDesde.HasValue)
                query = query.Where(v => v.Fecha >= fechaDesde.Value);
            if (fechaHasta.HasValue)
                query = query.Where(v => v.Fecha <= fechaHasta.Value);

            // Obtenemos el conteo total ANTES de paginar
            var totalCount = await query.CountAsync();

            // Aplicamos paginación
            var viaticos = await query
                .OrderByDescending(v => v.FechaRegistro)
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