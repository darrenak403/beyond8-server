namespace Beyond8.Common.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    Task RemoveAsync(string key);
}
