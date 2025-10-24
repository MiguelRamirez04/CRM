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
<<<<<<< HEAD
        private readonly ILogger<CacheService> _logger;
=======
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
>>>>>>> a0422e692ab83952e5b6e885f3174826d6ccb922

        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var json = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(json)) return default;
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch
            {
<<<<<<< HEAD
                _logger.LogWarning(ex, "⚠️ Error al obtener del caché Redis. Key: {Key}. La aplicación continuará sin caché.", key);
=======
                // Fall back seguro: si falla el cache, devolvemos null para que el flujo normal haga la consulta
>>>>>>> a0422e692ab83952e5b6e885f3174826d6ccb922
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(expiration);
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                await _cache.SetStringAsync(key, json, options);
            }
            catch
            {
<<<<<<< HEAD
                _logger.LogWarning(ex, "⚠️ Error al guardar en el caché Redis. Key: {Key}. La aplicación continuará sin caché.", key);
=======
                // Ignorar fallo de cache para no romper la respuesta
>>>>>>> a0422e692ab83952e5b6e885f3174826d6ccb922
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogDebug("🗑️ Clave eliminada del caché: {Key}", key);
            }
            catch
            {
<<<<<<< HEAD
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
                _logger.LogWarning(ex, "⚠️ Error al verificar existencia en caché. Key: {Key}", key);
                return false;
=======
                // Ignorar
>>>>>>> a0422e692ab83952e5b6e885f3174826d6ccb922
            }
        }
    }
}
