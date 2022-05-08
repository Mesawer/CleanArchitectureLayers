namespace Mesawer.ApplicationLayer.Interfaces;

public interface IStorageLocationService
{
    string GetPath(string type);
    string GetUrl(string type);
    string GetUrl(string type, string uri);
}
