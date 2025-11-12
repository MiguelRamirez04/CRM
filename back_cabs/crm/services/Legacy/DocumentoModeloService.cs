// =====================================================================================
// DOCUMENTO MODELO SERVICE - DocumentoModeloService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa la lógica de negocio para documentos modelo con cache Redis integrado.
// Utiliza Cache-Aside Pattern para optimización de performance.
//
// CACHE STRATEGY:
// 1. CHECK cache con key específica
// 2. CACHE HIT → Retornar datos cacheados (fast path)
// 3. CACHE MISS → Query BD → Mapear DTO → SET cache → Retornar datos
// 4. Si Redis falla → Fallback a BD directamente (graceful degradation)
//
// TTL CONFIGURATION:
// - GetAll: 30 minutos (datos estables, crítico estar actualizado)
// - Search: 60 minutos (búsquedas repetitivas de usuarios)
//
// PERFORMANCE:
// - Cache HIT: ~10-50ms (95% mejora)
// - Cache MISS: ~300-500ms (query + serialize + set)
// - Reducción carga BD: 80-90% esperado
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using back_cabs.CRM.services;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio de lógica de negocio para documentos modelo
    /// Implementa cache Redis con Cache-Aside Pattern
    /// </summary>
    public class DocumentoModeloService : IDocumentoModeloService
    {
        private readonly IDocumentoModeloRepository _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<DocumentoModeloService> _logger;

        // ═══════════════════════════════════════════════════════════════
        // CONFIGURACIÓN DE CACHE REDIS
        // ═══════════════════════════════════════════════════════════════

        private const string CACHE_KEY_ALL = "documentos_modelo:all";
        private const string CACHE_KEY_SEARCH_PREFIX = "documentos_modelo:search:";
        private static readonly TimeSpan CACHE_TTL_ALL = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan CACHE_TTL_SEARCH = TimeSpan.FromMinutes(60);

        public DocumentoModeloService(
            IDocumentoModeloRepository repository,
            ICacheService cacheService,
            ILogger<DocumentoModeloService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ─────────────────────────────────────────────────────────────────
        // 📖 MÉTODOS DE CONSULTA CON CACHE
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todos los documentos modelo activos con cache Redis
        /// </summary>
        public async Task<IEnumerable<DocumentoModeloResponseDto>> GetAllDocumentoModelosAsync()
        {
            try
            {
                // ✅ PASO 1: Intentar obtener desde cache
                _logger.LogDebug("🔍 Verificando cache Redis con key: {CacheKey}", CACHE_KEY_ALL);

                var cachedData = await _cacheService.GetAsync<List<DocumentoModeloResponseDto>>(CACHE_KEY_ALL);

                if (cachedData != null && cachedData.Any())
                {
                    _logger.LogInformation("✅ CACHE HIT para {CacheKey} - Retornando {Count} documentos modelo desde Redis",
                        CACHE_KEY_ALL, cachedData.Count);
                    return cachedData;
                }

                // ✅ PASO 2: CACHE MISS - Consultar base de datos
                _logger.LogInformation("⚠️ CACHE MISS para {CacheKey} - Consultando base de datos", CACHE_KEY_ALL);

                var documentosModelo = await _repository.GetAllDocumentoModelosAsync();
                var documentosModeloDto = documentosModelo.Select(MapToDto).ToList();

                // ✅ PASO 3: Guardar en cache para futuras consultas
                if (documentosModeloDto.Any())
                {
                    await _cacheService.SetAsync(CACHE_KEY_ALL, documentosModeloDto, CACHE_TTL_ALL);
                    _logger.LogInformation("💾 Cache SET para {CacheKey} con {Count} documentos modelo. TTL: {TTL} minutos",
                        CACHE_KEY_ALL, documentosModeloDto.Count, CACHE_TTL_ALL.TotalMinutes);
                }

                return documentosModeloDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener documentos modelo (GetAll). Fallback a BD sin cache.");

                // Fallback: Si todo falla, intentar consultar BD directamente
                var documentosModelo = await _repository.GetAllDocumentoModelosAsync();
                return documentosModelo.Select(MapToDto);
            }
        }

        /// <summary>
        /// Busca documentos modelo por término con cache Redis
        /// </summary>
        public async Task<IEnumerable<DocumentoModeloResponseDto>> SearchDocumentoModelosAsync(string searchTerm)
        {
            try
            {
                // Validar término de búsqueda
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("⚠️ Término de búsqueda vacío, retornando lista vacía");
                    return Enumerable.Empty<DocumentoModeloResponseDto>();
                }

                // Normalizar término para cache key (lowercase para evitar duplicados)
                var normalizedTerm = searchTerm.Trim().ToLowerInvariant();
                var cacheKey = $"{CACHE_KEY_SEARCH_PREFIX}{normalizedTerm}";

                // ✅ PASO 1: Intentar obtener desde cache
                _logger.LogDebug("🔍 Verificando cache Redis con key: {CacheKey}", cacheKey);

                var cachedData = await _cacheService.GetAsync<List<DocumentoModeloResponseDto>>(cacheKey);

                if (cachedData != null)
                {
                    _logger.LogInformation("✅ CACHE HIT para búsqueda '{SearchTerm}' - Retornando {Count} documentos modelo desde Redis",
                        searchTerm, cachedData.Count);
                    return cachedData;
                }

                // ✅ PASO 2: CACHE MISS - Consultar base de datos
                _logger.LogInformation("⚠️ CACHE MISS para búsqueda '{SearchTerm}' - Consultando base de datos", searchTerm);

                var documentosModelo = await _repository.SearchDocumentoModelosByFilterAsync(searchTerm);
                var documentosModeloDto = documentosModelo.Select(MapToDto).ToList();

                // ✅ PASO 3: Guardar en cache (incluso si está vacío - evita queries repetidas)
                await _cacheService.SetAsync(cacheKey, documentosModeloDto, CACHE_TTL_SEARCH);
                _logger.LogInformation("💾 Cache SET para búsqueda '{SearchTerm}' con {Count} documentos modelo. TTL: {TTL} minutos",
                    searchTerm, documentosModeloDto.Count, CACHE_TTL_SEARCH.TotalMinutes);

                return documentosModeloDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar documentos modelo con término '{SearchTerm}'. Fallback a BD sin cache.", searchTerm);

                // Fallback: Si todo falla, intentar consultar BD directamente
                var documentosModelo = await _repository.SearchDocumentoModelosByFilterAsync(searchTerm);
                return documentosModelo.Select(MapToDto);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 🔄 MAPEO ENTIDAD → DTO
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Mapea una entidad DocumentoModelo a DocumentoModeloResponseDto
        /// </summary>
        private static DocumentoModeloResponseDto MapToDto(DocumentoModelo documentoModelo)
        {
            return new DocumentoModeloResponseDto
            {
                Id = documentoModelo.Id,
                LegacyDocumentoModeloId = documentoModelo.LegacyDocumentoModeloId,
                Descripcion = documentoModelo.Descripcion,
                Naturaleza = documentoModelo.Naturaleza,
                AfectaExistencia = documentoModelo.AfectaExistencia,
                NoFolio = documentoModelo.NoFolio,
                ConceptoAsumidoId = documentoModelo.ConceptoAsumidoId,
                Activo = documentoModelo.Activo
            };
        }

        // ─────────────────────────────────────────────────────────────────
        // 🗑️ MÉTODOS DE INVALIDACIÓN DE CACHE (Para futuro POST/PUT/DELETE)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Invalida el cache de documentos modelo (para usar en operaciones de escritura)
        /// </summary>
        /// <remarks>
        /// Llamar este método después de:
        /// - POST /api/DocumentoModelo (crear documento modelo)
        /// - PUT /api/DocumentoModelo/{id} (actualizar documento modelo)
        /// - DELETE /api/DocumentoModelo/{id} (eliminar/desactivar documento modelo)
        /// </remarks>
        private async Task InvalidateCacheAsync()
        {
            try
            {
                await _cacheService.RemoveAsync(CACHE_KEY_ALL);
                _logger.LogInformation("🗑️ Cache invalidado: {CacheKey}", CACHE_KEY_ALL);

                // Nota: Para invalidar búsquedas específicas, se requeriría un sistema más complejo
                // Por ahora, las búsquedas expiran por TTL automáticamente
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error al invalidar cache de documentos modelo");
            }
        }
    }
}
