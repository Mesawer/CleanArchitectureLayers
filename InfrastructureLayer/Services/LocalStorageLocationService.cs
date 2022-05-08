using System.IO;
using Mesawer.ApplicationLayer;
using Mesawer.ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Mesawer.InfrastructureLayer.Services;

public class LocalStorageLocationService : IStorageLocationService
{
    private readonly IWebHostEnvironment _env;
    private readonly string              _baseUrl;

    public LocalStorageLocationService(IWebHostEnvironment env, IHttpContextAccessor accessor)
    {
        _env = env;

        var request = accessor.HttpContext?.Request;
        _baseUrl = $"{request?.Scheme}://{request?.Host}";
    }

    public string GetPath(string type) => Path.Combine(_env.WebRootPath, type);

    public string GetUrl(string type) => Url.Combine(_baseUrl, type);

    public string GetUrl(string type, string uri) => Url.Combine(_baseUrl, type, uri);
}
