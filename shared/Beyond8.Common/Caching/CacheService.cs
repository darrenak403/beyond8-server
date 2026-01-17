using System;
using System.Text.Json;
using StackExchange.Redis;

namespace Beyond8.Common.Caching;

public class CacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly string _prefix;

    public CacheService(IDatabase database, string serviceName)
    {
        _database = database;
        _prefix = $"{serviceName}:";
    }

    private string GetFullKey(string key) => $"{_prefix}{key}";

    public async Task<T?> GetAsync<T>(string key)
    {
        var fullKey = GetFullKey(key);
        var value = await _database.StringGetAsync(fullKey);

        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var fullKey = GetFullKey(key);

        var jsonValue = JsonSerializer.Serialize(value);

        await _database.StringSetAsync(fullKey, jsonValue, expiry);
    }

    public async Task RemoveAsync(string key)
    {
        var fullKey = GetFullKey(key);
        await _database.KeyDeleteAsync(fullKey);
    }
}