using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.PresentationLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Mesawer.PresentationLayer.Helpers;

[ApiController]
[Route("api/[controller]")]
[ApiConventionType(typeof(ApiConventions))]
public abstract class ApiControllerBase : ControllerBase
{
    private IMediator _mediator;

    protected IMediator Mediator
        => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()
                         ?? throw new ArgumentNullException(nameof(Mediator), "Mediator service not registered");

    protected async Task<ActionResult<T>> Execute<T>(IRequest<T> request, CancellationToken token) where T : class
        => request is { }
            ? Ok(await Mediator.Send(request, token))
            : StatusCode(StatusCodes.Status405MethodNotAllowed);

    protected async Task<ActionResult<HttpResult<T>>> Execute<T>(IRequest<T> request) where T : IConvertible
        => request is { }
            ? Ok(HttpResult<T>.Create(await Mediator.Send(request)))
            : StatusCode(StatusCodes.Status405MethodNotAllowed);

    protected async Task<ActionResult> Execute(IRequest request)
    {
        if (request is null) return StatusCode(StatusCodes.Status405MethodNotAllowed);

        await Mediator.Send(request);

        return NoContent();
    }

    protected async Task<ActionResult> Execute(ILongRunningRequest request)
    {
        if (request is null) return StatusCode(StatusCodes.Status405MethodNotAllowed);

        await Mediator.Send(request);

        return Accepted();
    }

    protected async Task<ActionResult<T>> Execute<T>(ILongRunningRequest<T> request, CancellationToken token)
        where T : class
        => request is { }
            ? Accepted(await Mediator.Send(request, token))
            : StatusCode(StatusCodes.Status405MethodNotAllowed);

    protected async Task<ActionResult<HttpResult<T>>> Execute<T>(ILongRunningRequest<T> request)
        where T : IConvertible
        => request is { }
            ? Accepted(HttpResult<T>.Create(await Mediator.Send(request)))
            : StatusCode(StatusCodes.Status405MethodNotAllowed);
}
