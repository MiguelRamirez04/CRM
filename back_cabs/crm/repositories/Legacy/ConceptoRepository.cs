// =====================================================================================
// CONCEPTO REPOSITORY - ConceptoRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa el repositorio de conceptos con EF Core optimizado.
// Consulta la vista VwConceptosCompletos con filtros eficientes.
//
// OPTIMIZACIONES:
// - AsNoTracking(): No trackea entidades para mejor performance
// - Where Activo=true: Solo conceptos activos
// - EF.Functions.Like: Búsqueda case-insensitive en CodigoConcepto y NombreConcepto
// - ToListAsync(): Ejecución diferida optimizada
//
// =====================================================================================

using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models;
using back_cabs.CRM.contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.repositories.Legacy
{
    /// <summary>
    /// Repositorio para conceptos usando EF Core optimizado
    /// Consulta VwConceptosCompletos con filtros eficientes
    /// </summary>
    public class ConceptoRepository : IConceptoRepository
    {
        private readonly ReadOnlyContext _context;
        private readonly ILogger<ConceptoRepository> _logger;

        public ConceptoRepository(
            ReadOnlyContext context,
            ILogger<ConceptoRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ─────────────────────────────────────────────────────────────────
        // 📖 MÉTODOS DE CONSULTA
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todos los conceptos activos desde VwConceptosCompletos
        /// </summary>
        public async Task<IEnumerable<VwConceptosCompletos>> GetAllConceptosAsync()
        {
            try
            {
                _logger.LogDebug("Consultando todos los conceptos activos desde VwConceptosCompletos");

                var conceptos = await _context.VwConceptosCompletos
                    .AsNoTracking()
                    .Where(c => c.Activo)
                    .OrderBy(c => c.ConceptoId)
                    .ToListAsync();

                _logger.LogInformation("Se encontraron {Count} conceptos activos", conceptos.Count);
                return conceptos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar conceptos desde VwConceptosCompletos");
                throw;
            }
        }

        /// <summary>
        /// Busca conceptos por término de búsqueda en CodigoConcepto o NombreConcepto
        /// </summary>
        public async Task<IEnumerable<VwConceptosCompletos>> SearchConceptosByFilterAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("Término de búsqueda vacío, retornando lista vacía");
                    return Enumerable.Empty<VwConceptosCompletos>();
                }

                var normalizedTerm = $"%{searchTerm.Trim()}%";
                _logger.LogDebug("Buscando conceptos con término: {SearchTerm}", searchTerm);

                var conceptos = await _context.VwConceptosCompletos
                    .AsNoTracking()
                    .Where(c => c.Activo &&
                           (EF.Functions.Like(c.CodigoConcepto, normalizedTerm) ||
                            EF.Functions.Like(c.NombreConcepto, normalizedTerm)))
                    .OrderBy(c => c.ConceptoId)
                    .ToListAsync();

                _logger.LogInformation("Búsqueda '{SearchTerm}' encontró {Count} conceptos", searchTerm, conceptos.Count);
                return conceptos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar conceptos con término '{SearchTerm}'", searchTerm);
                throw;
            }
        }
    }
}
