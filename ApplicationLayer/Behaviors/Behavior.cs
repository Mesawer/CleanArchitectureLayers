using System.Collections.Generic;
using System.Text;
using Mesawer.ApplicationLayer.Interfaces;
using Microsoft.Extensions.Logging;

namespace Mesawer.ApplicationLayer.Behaviors;

public delegate void Log(string message, params object[] args);

public class Behavior<TRequest>
{
    private readonly ILogger<TRequest>       _logger;
    private readonly IApplicationUserService _currentUser;

    public Behavior(ILogger<TRequest> logger) => _logger = logger;

    public Behavior(
        ILogger<TRequest> logger,
        IApplicationUserService currentUser)
    {
        _logger      = logger;
        _currentUser = currentUser;
    }

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
        var args = new List<object>
        {
            requestName,
            request
        };

        // Part 1: Request Logging
        message.Append($"{"Request",7}: ({{ReqName}}), {{Req}}");

        // Part 2: User Data Logging
        if (_currentUser?.Id is not null)
        {
            var userId   = _currentUser.Id ?? string.Empty;
            var userName = _currentUser.UserName ?? string.Empty;
            var email    = _currentUser.Email ?? string.Empty;

            message.Append($"\n{"User",7}: {{UserId}}, {{Username}}, {{Email}}");

            args.Add(userId);
            args.Add(userName);
            args.Add(email);
        }

        // Part 3: Extras Logging
        if (extra is not null)
        {
            foreach (var (k, value) in extra)
            {
                message.Append($"\n{k}: {{{k}}}");
                args.Add(value);
            }
        }

        action(message.ToString(), args.ToArray());
    }
}
