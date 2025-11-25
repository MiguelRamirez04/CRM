using Microsoft.Extensions.Caching.Distributed;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace back_cabs.CRM.services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task RemoveByPrefixAsync(string prefix);
        Task<CacheStatistics> GetStatisticsAsync();
    }

    public class CacheStatistics
    {
        public int HitCount { get; set; }
        public int MissCount { get; set; }
        public int ErrorCount { get; set; }
        public double HitRate => (HitCount + MissCount) > 0 ? (double)HitCount / (HitCount + MissCount) * 100 : 0;
    }

    /// <summary>
    /// Servicio profesional de caché distribuido con Redis
    /// Características:
    /// - Compresión automática para payloads >1KB
    /// - Manejo robusto de errores con graceful degradation
    /// - Métricas de rendimiento
    /// - Logging detallado con emojis para mejor visibilidad
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        
        // Métricas en memoria (thread-safe)
        private int _hitCount = 0;
        private int _missCount = 0;
        private int _errorCount = 0;
        private const int COMPRESSION_THRESHOLD = 1024; // 1KB

        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false // Compacto para menor tamaño
            };
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                byte[]? cachedBytes = await _cache.GetAsync(key);

                if (cachedBytes == null || cachedBytes.Length == 0)
                {
                    Interlocked.Increment(ref _missCount);
                    _logger.LogDebug("❌ Cache MISS: {Key}", key);
                    return default;
                }

                string jsonValue;
                
                // Detectar si está comprimido (primer byte es el flag)
                if (cachedBytes[0] == 1)
                {
                    // Descomprimir
                    using var compressedStream = new MemoryStream(cachedBytes, 1, cachedBytes.Length - 1);
                    using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
                    using var resultStream = new MemoryStream();
                    await gzipStream.CopyToAsync(resultStream);
                    jsonValue = Encoding.UTF8.GetString(resultStream.ToArray());
                }
                else
                {
                    // Sin comprimir
                    jsonValue = Encoding.UTF8.GetString(cachedBytes, 1, cachedBytes.Length - 1);
                }

                var result = JsonSerializer.Deserialize<T>(jsonValue, _jsonOptions);
                
                Interlocked.Increment(ref _hitCount);
                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogInformation("✅ Cache HIT: {Key} ({Elapsed:F2}ms, {Size}KB)", 
                    key, elapsed, cachedBytes.Length / 1024.0);
                
                return result;
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _errorCount);
                _logger.LogWarning(ex, "⚠️ Error recuperando del caché: {Key}. Continuando sin caché.", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                
                // Serializar a JSON
                string jsonValue = JsonSerializer.Serialize(value, _jsonOptions);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonValue);
                
                byte[] finalBytes;
                bool compressed = false;

                // Comprimir si supera el umbral
                if (jsonBytes.Length > COMPRESSION_THRESHOLD)
                {
                    using var outputStream = new MemoryStream();
                    using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Fastest))
                    {
                        await gzipStream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
                    }
                    
                    var compressedData = outputStream.ToArray();
                    finalBytes = new byte[compressedData.Length + 1];
                    finalBytes[0] = 1; // Flag de compresión
                    Buffer.BlockCopy(compressedData, 0, finalBytes, 1, compressedData.Length);
                    compressed = true;
                }
                else
                {
                    finalBytes = new byte[jsonBytes.Length + 1];
                    finalBytes[0] = 0; // Flag sin compresión
                    Buffer.BlockCopy(jsonBytes, 0, finalBytes, 1, jsonBytes.Length);
                }

                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(expiration)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(Math.Min(expiration.TotalMinutes / 2, 5)));

                await _cache.SetAsync(key, finalBytes, options);

                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                var compressionRatio = compressed ? (1 - (double)finalBytes.Length / jsonBytes.Length) * 100 : 0;
                
                _logger.LogInformation("💾 Cache SET: {Key} ({Elapsed:F2}ms, {Size}KB{Compression})", 
                    key, 
                    elapsed, 
                    finalBytes.Length / 1024.0,
                    compressed ? $", comprimido {compressionRatio:F1}%" : "");
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _errorCount);
                _logger.LogWarning(ex, "⚠️ Error guardando en caché: {Key}. Continuando sin caché.", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogDebug("🗑️ Clave eliminada del caché: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error al eliminar del caché Redis. Key: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var value = await _cache.GetStringAsync(key);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al verificar existencia en caché. Key: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// Elimina todas las claves con el prefijo especificado.
        /// NOTA: IDistributedCache no soporta búsqueda por patrón nativamente.
        /// Para implementación completa se requiere acceso directo a Redis con SCAN.
        /// </summary>
        public Task RemoveByPrefixAsync(string prefix)
        {
            _logger.LogWarning("⚠️ RemoveByPrefixAsync no implementado: IDistributedCache no soporta pattern matching. Usar implementación Redis directa si es crítico.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Obtiene estadísticas actuales del caché
        /// </summary>
        public Task<CacheStatistics> GetStatisticsAsync()
        {
            var stats = new CacheStatistics
            {
                HitCount = _hitCount,
                MissCount = _missCount,
                ErrorCount = _errorCount
            };

            _logger.LogInformation("📊 Estadísticas caché - Hit: {Hit}, Miss: {Miss}, Error: {Error}, Hit Rate: {HitRate:F1}%",
                stats.HitCount, stats.MissCount, stats.ErrorCount, stats.HitRate);

            return Task.FromResult(stats);
        }
    }
}