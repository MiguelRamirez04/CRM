using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace back_cabs.CRM.services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                string? cachedValue = await _cache.GetStringAsync(key);

                if (string.IsNullOrEmpty(cachedValue))
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error al obtener del caché Redis. Key: {Key}. La aplicación continuará sin caché.", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(expiration);

                string jsonValue = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, jsonValue, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error al guardar en el caché Redis. Key: {Key}. La aplicación continuará sin caché.", key);
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
    }
}