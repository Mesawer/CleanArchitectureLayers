using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using JetBrains.Annotations;
using Mesawer.ApplicationLayer.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Mesawer.ApplicationLayer.Behaviors;

public delegate void Log(string message, params object[] args);

[PublicAPI]
public class Behavior<TRequest>
{
    private readonly ILogger<Behavior<TRequest>> _logger;
    private readonly IHttpContextAccessor        _httpContextAccessor;

    public Behavior(ILogger<Behavior<TRequest>> logger) => _logger = logger;

    public Behavior(ILogger<Behavior<TRequest>> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger              = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User;
    private string Ip => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    public void LogInformation(string key, TRequest request, Dictionary<string, object> extra = null)
        => Log(key, request, _logger.LogInformation, extra);

    public void LogWarning(string key, TRequest request, Dictionary<string, object> extra = null)
        => Log(key, request, _logger.LogWarning, extra);

    public void LogCritical(string key, TRequest request, Dictionary<string, object> extra = null)
        => Log(key, request, _logger.LogCritical, extra);

    public void LogError(string key, TRequest request, Dictionary<string, object> extra = null)
        => Log(key, request, _logger.LogError, extra);

    private void Log(string key, TRequest request, Log action, Dictionary<string, object> extra)
    {
        var requestName = typeof(TRequest).Name;

        var message = new StringBuilder($"## {key} ##\n");

        var anyFiles = typeof(TRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(c => c.PropertyType.IsOfType(typeof(IFormFile)) && c.GetValue(request) is not null);

        var req = !anyFiles
            ? JsonConvert.SerializeObject(request)
            : JsonConvert.SerializeObject(new { Issue = "Can't serialize due to object size" });

        var args = new List<object>
        {
            req
        };

        // Part 1: Request Logging
        message.Append($"{"Request",7}: ({requestName}), {{Req:j}}");

        // Part 2: User Data Logging
        var userId = User.FindFirst("sub")?.Value;

        message.Append($"\nUser: {userId}, IpAddress: {Ip}");

        // Part 3: Extras Logging
        if (extra is not null)
        {
            foreach (var (k, value) in extra)
            {
                if (value is null) continue;

                if (value.GetType().IsValueType)
                {
                    message.Append($"\n{k}: {{{k}}}");
                }
                else
                {
                    message.Append($"\n{k}: {{{k}:j}}");
                }

                args.Add(JsonConvert.SerializeObject(value));
            }
        }

        action(message.ToString(), args.ToArray());
    }
}
