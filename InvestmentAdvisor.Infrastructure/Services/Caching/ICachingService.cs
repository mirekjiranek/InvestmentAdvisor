using System;
using System.Threading.Tasks;

namespace Infrastructure.Services.Caching
{
    public interface ICachingService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}