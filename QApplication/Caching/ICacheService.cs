
using QApplication.Responses.AvailabilityResponse;

namespace QApplication.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null);

    Task RemoveAsync(string key);

    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null);
    
    Task<T?> HashGetAsync<T>(string key, string field);
    Task HashSetAsync<T>(string key, string field, T value, TimeSpan? expiry = null);
    Task HashRemoveAsync(string key);


}