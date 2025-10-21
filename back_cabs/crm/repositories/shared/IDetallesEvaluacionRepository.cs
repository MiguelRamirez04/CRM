using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces;
using back_cabs.CRM.models.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_cabs.CRM.Repositories
{
    public class DetalleEvaluacionRepository : IDetalleEvaluacionRepository
    {
        private readonly ReadOnlyContext _readOnlyContext;
        private readonly WriteContext _writeContext;

        public DetalleEvaluacionRepository(ReadOnlyContext readOnlyContext, WriteContext writeContext)
        {
            _readOnlyContext = readOnlyContext;
            _writeContext = writeContext;
        }

        public async Task<List<EvaluacionDetalle>> GetByEvaluacionIdAsync(int evaluacionId)
        {
            return await _readOnlyContext.EvaluacionesDetalles
                        .Where(d => d.EvaluacionId == evaluacionId)
                        .ToListAsync();
        }

        public async Task<EvaluacionDetalle?> GetByIdAsync(int id)
        {
            // Nota: Usamos ReadOnlyContext para lecturas
            return await _readOnlyContext.EvaluacionesDetalles.FindAsync(id);
        }

        public async Task<EvaluacionDetalle> CreateAsync(EvaluacionDetalle detalle)
        {
            _writeContext.EvaluacionesDetalles.Add(detalle);
            await _writeContext.SaveChangesAsync();
            return detalle; // EF Core actualiza el ID en la entidad 'detalle'
        }

        public async Task UpdateAsync(EvaluacionDetalle detalle)
        {
            // Asumimos que la entidad 'detalle' fue obtenida y modificada por el servicio.
            // EF Core rastreará los cambios y los guardará.
            _writeContext.EvaluacionesDetalles.Update(detalle);
            await _writeContext.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var detalleAEliminar = await _writeContext.EvaluacionesDetalles.FindAsync(id);
            if (detalleAEliminar == null)
            {
                return false;
            }

            _writeContext.EvaluacionesDetalles.Remove(detalleAEliminar);
            await _writeContext.SaveChangesAsync();
            return true;
        }
    }
}