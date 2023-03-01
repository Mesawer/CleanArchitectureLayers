using Mesawer.ApplicationLayer.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Mesawer.InfrastructureLayer.Services;

public class MemoryCacheService : IMemoryCacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache) => _cache = cache;

    public object Get(string key)
    {
        var exists = _cache.TryGetValue(key, out var token);

        return exists ? token : null;
    }

    public T Get<T>(string key)
    {
        var exists = _cache.TryGetValue(key, out var token);

        return exists ? (T) token : default;
    }

    public void Add(string key, object content) => _cache.Set(key, content);

    public void Remove(string key) => _cache.Remove(key);
}
