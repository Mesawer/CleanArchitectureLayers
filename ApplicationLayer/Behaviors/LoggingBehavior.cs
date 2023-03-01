using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Mesawer.ApplicationLayer.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : Behavior<TRequest>, IRequestPostProcessor<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IHttpContextAccessor httpContextAccessor) : base(logger, httpContextAccessor) { }


    public Task Process(TRequest request, TResponse response, CancellationToken ct)
    {
        LogInformation("REQUEST_LOGGER", request, new Dictionary<string, object> { { "Response", response } });

        return Task.CompletedTask;
    }
}
