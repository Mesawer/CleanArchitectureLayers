using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Mesawer.ApplicationLayer.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> : Behavior<TRequest>, IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        IHttpContextAccessor httpContextAccessor) : base(logger, httpContextAccessor)
        => _timer = new Stopwatch();

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
            LogCritical("LONG_RUNNING_REQUEST",
                request,
                new Dictionary<string, object> { { "ElapsedMilliseconds", elapsedMilliseconds } });

        return response;
    }
}
