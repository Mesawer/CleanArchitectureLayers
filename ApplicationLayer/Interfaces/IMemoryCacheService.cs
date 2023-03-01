using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Interfaces;

[PublicAPI]
public interface IMemoryCacheService
{
    object Get(string key);
    T Get<T>(string key);
    void Add(string key, object content);
    void Remove(string key);
}
