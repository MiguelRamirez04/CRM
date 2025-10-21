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

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
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
                // Manejar la excepción (log, etc.)
                Console.WriteLine($"Error al obtener del caché: {ex.Message}");
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
                // Manejar la excepción (log, etc.)
                Console.WriteLine($"Error al guardar en el caché: {ex.Message}");
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                // Manejar la excepción (log, etc.)
                Console.WriteLine($"Error al eliminar del caché: {ex.Message}");
            }
        }
    }
}