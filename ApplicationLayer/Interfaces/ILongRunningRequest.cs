using JetBrains.Annotations;
using MediatR;

namespace Mesawer.ApplicationLayer.Interfaces;

[PublicAPI]
public interface ILongRunningRequest : IRequest { }

[PublicAPI]
public interface ILongRunningRequest<out TResponse> : IRequest<TResponse> { }

[PublicAPI]
public interface ILongRunningRequestHandler<in TRequest> : IRequestHandler<TRequest, Unit>
    where TRequest : ILongRunningRequest { }

[PublicAPI]
public interface ILongRunningRequestHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : ILongRunningRequest<TResponse> { }
