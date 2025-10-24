using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace back_cabs.CRM.services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveAsync(string key);
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
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
                // Fall back seguro: si falla el cache, devolvemos null para que el flujo normal haga la consulta
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
                // Ignorar fallo de cache para no romper la respuesta
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch
            {
                // Ignorar
            }
        }
    }
}
