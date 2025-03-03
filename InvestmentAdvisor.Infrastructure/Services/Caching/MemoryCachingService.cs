using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Services.Caching
{
    public class MemoryCachingService : ICachingService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCachingService> _logger;
        private readonly CacheSettings _cacheSettings;

        public MemoryCachingService(
            IMemoryCache cache, 
            ILogger<MemoryCachingService> logger,
            IOptions<CacheSettings> cacheSettings)
        {
            _cache = cache;
            _logger = logger;
            _cacheSettings = cacheSettings.Value;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out T cachedResult))
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return cachedResult;
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            T result = await factory();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(
                    expiration ?? TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationMinutes));

            _cache.Set(key, result, cacheEntryOptions);
            return result;
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_cache.TryGetValue(key, out _));
        }
    }
}