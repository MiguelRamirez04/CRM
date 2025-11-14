using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.repositories.Legacy
{
    /// <summary>
    /// Repositorio para AdmDocumentos
    /// </summary>
    public class AdmDocumentoRepository : IAdmDocumentoRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmDocumentoRepository> _logger;

        public AdmDocumentoRepository(
            LegacyCompacReadOnlyContext context,
            ILogger<AdmDocumentoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Busca documentos aplicando filtros con paginación
        /// </summary>
        public async Task<(List<AdmDocumento> documentos, int totalRegistros)> SearchPaginatedAsync(AdmDocumentoFilterDto filter)
        {
            try
            {
                var query = _context.AdmDocumentos.AsNoTracking();

                // Aplicar filtros
                if (filter.FechaInicio.HasValue)
                {
                    query = query.Where(d => d.CFecha >= filter.FechaInicio.Value);
                }

                if (filter.FechaFin.HasValue)
                {
                    query = query.Where(d => d.CFecha <= filter.FechaFin.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.Folio))
                {
                    var folioStr = filter.Folio.Trim();
                    // Intentar convertir a double para búsqueda exacta
                    if (double.TryParse(folioStr, out double folioNum))
                    {
                        query = query.Where(d => d.CFolio == folioNum);
                    }
                }

                if (!string.IsNullOrWhiteSpace(filter.RazonSocial))
                {
                    var razonSocial = filter.RazonSocial.Trim().ToLower();
                    query = query.Where(d => d.CRazonSocial.ToLower().Contains(razonSocial));
                }

                if (filter.FechaVencimientoInicio.HasValue)
                {
                    query = query.Where(d => d.CFechaVencimiento >= filter.FechaVencimientoInicio.Value);
                }

                if (filter.FechaVencimientoFin.HasValue)
                {
                    query = query.Where(d => d.CFechaVencimiento <= filter.FechaVencimientoFin.Value);
                }

                if (filter.IdConcepto.HasValue)
                {
                    query = query.Where(d => d.CIdConceptoDocumento == filter.IdConcepto.Value);
                }

                if (filter.IdAgente.HasValue)
                {
                    query = query.Where(d => d.CIdAgente == filter.IdAgente.Value);
                }

                // Obtener total de registros antes de paginar
                var totalRegistros = await query.CountAsync();

                // Aplicar ordenamiento (más recientes primero)
                query = query.OrderByDescending(d => d.CFecha)
                            .ThenByDescending(d => d.CFolio);

                // Aplicar paginación
                var documentos = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                _logger.LogInformation(
                    "✅ Búsqueda de documentos completada. Total: {Total}, Página: {Page}, Tamaño: {PageSize}",
                    totalRegistros, filter.Page, filter.PageSize);

                return (documentos, totalRegistros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar documentos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un documento por ID
        /// </summary>
        public async Task<AdmDocumento?> GetByIdWithMovimientosAsync(int idDocumento)
        {
            try
            {
                var documento = await _context.AdmDocumentos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.CIdDocumento == idDocumento);

                return documento;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener documento {IdDocumento}", idDocumento);
                throw;
            }
        }

        /// <summary>
        /// Obtiene los movimientos de un documento específico
        /// </summary>
        public async Task<List<AdmMovimiento>> GetMovimientosByDocumentoIdAsync(int idDocumento)
        {
            try
            {
                var movimientos = await _context.AdmMovimientos
                    .AsNoTracking()
                    .Where(m => m.CIdDocumento == idDocumento)
                    .OrderBy(m => m.CNumeroMovimiento)
                    .ToListAsync();

                return movimientos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener movimientos del documento {IdDocumento}", idDocumento);
                throw;
            }
        }
    }
}
