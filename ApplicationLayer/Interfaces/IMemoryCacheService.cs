using Mesawer.ApplicationLayer.Enums;
using Mesawer.ApplicationLayer.Models;

namespace Mesawer.ApplicationLayer.Interfaces;

public interface IMemoryCacheService
{
    object Get(string key);
    TokenObject Get(TokenType type, string userId);
    void Add(string key, object content);
    void Add(string userId, TokenObject token);
    void Remove(string key);
    void Remove(TokenType type, string userId);

}
