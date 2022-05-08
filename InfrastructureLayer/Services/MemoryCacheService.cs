using Mesawer.ApplicationLayer.Enums;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.ApplicationLayer.Models;
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

    public TokenObject Get(TokenType type, string userId)
    {
        var obj = Get(GenerateTokenKey(type, userId));

        return obj as TokenObject;
    }

    public void Add(string key, object content) => _cache.Set(key, content);

    public void Add(string userId, TokenObject token)
        => _cache.Set(GenerateTokenKey(token.Type, userId), token, token.ExpiresAt);

    public void Remove(string key) => _cache.Remove(key);

    public void Remove(TokenType type, string userId) => _cache.Remove(GenerateTokenKey(type, userId));

    private static string GenerateTokenKey(TokenType type, string userId) => $"{type.ToString().ToLower()}-{userId}";
}
