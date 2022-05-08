using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mesawer.DomainLayer.Exceptions;
using MediatR;
using Mesawer.ApplicationLayer.Exceptions;
using Mesawer.ApplicationLayer.Resources.Common;
using Microsoft.Extensions.Logging;

namespace Mesawer.ApplicationLayer.Behaviors;

public class UnhandledExceptionBehavior<TRequest, TResponse>
    : Behavior<TRequest>, IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public UnhandledExceptionBehavior(ILogger<TRequest> logger) : base(logger) { }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            return await next();
        }
        catch (DomainException e)
        {
            LogError("DOMAIN_EXCEPTION", request, new Dictionary<string, object> { { "Error", e } });

            throw new BadRequestException(SharedRes.DomainExceptionMessage);
        }
        catch (NotImplementedException)
        {
            throw new BadRequestException(SharedRes.NotImplemented);
        }
        catch (NotFoundException e)
        {
            LogError("NOT_FOUND", request, new Dictionary<string, object> { { "Exception", e } });

            throw;
        }
        catch (BadRequestException e)
        {
            LogError("BAD_REQUEST", request, new Dictionary<string, object> { { "Error", e } });

            throw;
        }
        catch (ForbiddenAccessException e)
        {
            LogError("FORBIDDEN_ACCESS", request, new Dictionary<string, object> { { "Exception", e } });

            throw;
        }
        catch (UnauthorizedAccessException e)
        {
            LogError("UNAUTHORIZED_ACCESS", request, new Dictionary<string, object> { { "Exception", e } });

            throw;
        }
        catch (Exception e) when (e is not ValidationException)
        {
            LogError("UNHANDLED_EXC", request, new Dictionary<string, object> { { "Exception", e } });

            throw new UnhandledRequestException($"Unhandled Request Exception in {typeof(TRequest).Name}", e);
        }
    }
}
