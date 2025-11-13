// =====================================================================================
// SERVICIO NÚMERO SERIE CON REDIS CACHE - NumeroSerieService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa la lógica de negocio para números de serie con cache Redis.
// Aplica Cache-Aside Pattern para optimizar rendimiento de consultas.
//
// CACHE STRATEGY:
// - GetAll: TTL 30min (datos cambian poco, cache más corto)
// - Search: TTL 60min (búsquedas específicas, cache más largo)
// - Keys: "numeros_serie:all", "numeros_serie:search:{term}"
//
// FLUJO CACHE-ASIDE:
// 1. Verificar cache → HIT: retornar desde Redis
// 2. MISS: consultar BD → guardar en cache → retornar datos
// 3. Error: fallback directo a BD sin cache
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.Interfaces.Shared;
using back_cabs.CRM.models;
using CRM.DTOs.Response;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio para gestión de números de serie con cache Redis
    /// Implementa Cache-Aside Pattern para optimización de consultas
    /// </summary>
    public class NumeroSerieService : INumeroSerieService
    {
        private readonly INumeroSerieRepository _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<NumeroSerieService> _logger;

        // ─────────────────────────────────────────────────────────────────
        // CONFIGURACIÓN DE CACHE
        // ─────────────────────────────────────────────────────────────────

        private const string CACHE_KEY_ALL = "numeros_serie:all";
        private const string CACHE_KEY_SEARCH_PREFIX = "numeros_serie:search:";
        private static readonly TimeSpan CACHE_TTL_ALL = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan CACHE_TTL_SEARCH = TimeSpan.FromMinutes(60);

        public NumeroSerieService(
            INumeroSerieRepository repository,
            ICacheService cacheService,
            ILogger<NumeroSerieService> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────
        // 📄 PAGINACIÓN CON CACHE (30 MIN TTL)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene números de serie paginados con cache Redis
        /// Cache Key: numeros_serie:page:{page}:size:{pageSize}
        /// </summary>
        public async Task<PaginatedResponseDto<NumeroSerieResponseDto>> GetNumeroSeriePaginatedAsync(int page, int pageSize)
        {
            try
            {
                // Validación de parámetros
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 30;
                if (pageSize > 100) pageSize = 100; // Límite máximo

                var cacheKey = $"{CACHE_KEY_ALL}:page:{page}:size:{pageSize}";

                // ✅ PASO 1: Verificar cache
                _logger.LogDebug("🔍 Verificando cache Redis paginado con key: {CacheKey}", cacheKey);

                var cachedData = await _cacheService.GetAsync<PaginatedResponseDto<NumeroSerieResponseDto>>(cacheKey);

                if (cachedData != null)
                {
                    _logger.LogInformation("✅ CACHE HIT página {Page} - Retornando {Count} de {TotalItems} números de serie desde Redis",
                        page, cachedData.Items.Count(), cachedData.TotalItems);
                    return cachedData;
                }

                // ✅ PASO 2: CACHE MISS - Consultar base de datos
                _logger.LogInformation("⚠️ CACHE MISS para página {Page} tamaño {PageSize} - Consultando base de datos", page, pageSize);

                var (data, totalRecords) = await _repository.GetNumeroSeriePaginatedAsync(page, pageSize);
                var numeroSerieDto = data.Select(MapToDto).ToList();

                // Crear respuesta paginada
                var paginatedResponse = new PaginatedResponseDto<NumeroSerieResponseDto>
                {
                    Items = numeroSerieDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };

                // ✅ PASO 3: Guardar en cache
                await _cacheService.SetAsync(cacheKey, paginatedResponse, CACHE_TTL_ALL);
                _logger.LogInformation("💾 Cache SET para página {Page} tamaño {PageSize} con {Count} de {TotalItems} números de serie. TTL: {TTL} minutos",
                    page, pageSize, numeroSerieDto.Count, totalRecords, CACHE_TTL_ALL.TotalMinutes);

                return paginatedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener números de serie paginados (Página {Page}, Tamaño {PageSize}). Fallback a BD sin cache.", page, pageSize);

                // Fallback sin cache
                var (data, totalRecords) = await _repository.GetNumeroSeriePaginatedAsync(page, pageSize);
                var numeroSerieDto = data.Select(MapToDto).ToList();
                return new PaginatedResponseDto<NumeroSerieResponseDto>
                {
                    Items = numeroSerieDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };
            }
        }

        /// <summary>
        /// Busca números de serie por término con paginación y cache Redis
        /// Cache Key: numeros_serie:search:{searchTerm}:page:{page}:size:{pageSize}
        /// </summary>
        public async Task<PaginatedResponseDto<NumeroSerieResponseDto>> SearchNumeroSeriePaginatedAsync(string searchTerm, int page, int pageSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("Término de búsqueda vacío, retornando lista vacía");
                    return new PaginatedResponseDto<NumeroSerieResponseDto>
                    {
                        Items = new List<NumeroSerieResponseDto>(),
                        Pagina = page,
                        ResultadosPorPagina = pageSize,
                        TotalItems = 0
                    };
                }

                // Validación de parámetros
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 30;
                if (pageSize > 100) pageSize = 100;

                var normalizedTerm = searchTerm.Trim().ToLower();
                var cacheKey = $"{CACHE_KEY_SEARCH_PREFIX}{normalizedTerm}:page:{page}:size:{pageSize}";

                // ✅ PASO 1: Verificar cache
                _logger.LogDebug("🔍 Verificando cache Redis búsqueda paginada con key: {CacheKey}", cacheKey);

                var cachedData = await _cacheService.GetAsync<PaginatedResponseDto<NumeroSerieResponseDto>>(cacheKey);

                if (cachedData != null)
                {
                    _logger.LogInformation("✅ CACHE HIT para búsqueda '{SearchTerm}' página {Page} - Retornando {Count} de {TotalRecords} números de serie desde Redis",
                        searchTerm, page, cachedData.Items.Count(), cachedData.TotalItems);
                    return cachedData;
                }

                // ✅ PASO 2: CACHE MISS - Consultar base de datos
                _logger.LogInformation("⚠️ CACHE MISS para búsqueda '{SearchTerm}' página {Page} tamaño {PageSize} - Consultando base de datos", 
                    searchTerm, page, pageSize);

                var (data, totalRecords) = await _repository.SearchNumeroSeriePaginatedAsync(searchTerm, page, pageSize);
                var numeroSerieDto = data.Select(MapToDto).ToList();

                // Crear respuesta paginada
                var paginatedResponse = new PaginatedResponseDto<NumeroSerieResponseDto>
                {
                    Items = numeroSerieDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };

                // ✅ PASO 3: Guardar en cache
                await _cacheService.SetAsync(cacheKey, paginatedResponse, CACHE_TTL_SEARCH);
                _logger.LogInformation("💾 Cache SET para búsqueda '{SearchTerm}' página {Page} con {Count} de {TotalRecords} números de serie. TTL: {TTL} minutos",
                    searchTerm, page, numeroSerieDto.Count, totalRecords, CACHE_TTL_SEARCH.TotalMinutes);

                return paginatedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar números de serie paginados ('{SearchTerm}', Página {Page}, Tamaño {PageSize}). Fallback a BD sin cache.", 
                    searchTerm, page, pageSize);

                // Fallback sin cache
                var (data, totalRecords) = await _repository.SearchNumeroSeriePaginatedAsync(searchTerm, page, pageSize);
                var numeroSerieDto = data.Select(MapToDto).ToList();
                return new PaginatedResponseDto<NumeroSerieResponseDto>
                {
                    Items = numeroSerieDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 🔄 MAPEO ENTIDAD → DTO
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Mapea una entidad VwNumerosSerieCompletos a NumeroSerieResponseDto
        /// </summary>
        private static NumeroSerieResponseDto MapToDto(VwNumerosSerieCompletos numeroSerie)
        {
            return new NumeroSerieResponseDto
            {
                NumeroSerieId = numeroSerie.NumeroSerieId,
                NumeroSerie = numeroSerie.NumeroSerie,
                Estado = numeroSerie.Estado,
                EstadoAnterior = numeroSerie.EstadoAnterior,
                Costo = numeroSerie.Costo,
                FechaTimestamp = numeroSerie.FechaTimestamp,
                Activo = numeroSerie.Activo,

                // Datos Legacy
                LegacySerieId = numeroSerie.LegacySerieId,
                LegacyProductoId = numeroSerie.LegacyProductoId,
                LegacyAlmacenId = numeroSerie.LegacyAlmacenId,
                ProductoLegacy = numeroSerie.ProductoLegacy,
                NumeroSerieLegacy = numeroSerie.NumeroSerieLegacy,
                AlmacenLegacy = numeroSerie.AlmacenLegacy,
                EstadoLegacy = numeroSerie.EstadoLegacy,
                EstadoAnteriorLegacy = numeroSerie.EstadoAnteriorLegacy,
                CostoLegacy = numeroSerie.CostoLegacy,
                TimestampLegacy = numeroSerie.TimestampLegacy
            };
        }
    }
}

