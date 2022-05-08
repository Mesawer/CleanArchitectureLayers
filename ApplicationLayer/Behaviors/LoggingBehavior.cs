using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;
using Mesawer.ApplicationLayer.Interfaces;
using Microsoft.Extensions.Logging;

namespace Mesawer.ApplicationLayer.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : Behavior<TRequest>, IRequestPostProcessor<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public LoggingBehavior(
        ILogger<TRequest> logger,
        IApplicationUserService currentUser) : base(logger, currentUser) { }

    public Task Process(TRequest request, TResponse response, CancellationToken ct)
    {
        LogInformation("REQUEST_LOGGER", request, new Dictionary<string, object> { { "Response", response } });

        return Task.CompletedTask;
    }
}
