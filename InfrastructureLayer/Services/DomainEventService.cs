using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.DomainLayer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mesawer.InfrastructureLayer.Services;

public class DomainEventService : IDomainEventService
{
    private readonly IServiceProvider            _serviceProvider;
    private readonly ILogger<DomainEventService> _logger;

    public DomainEventService(IServiceProvider serviceProvider, ILogger<DomainEventService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger          = logger;
    }

    public Task Publish(string fullName, string serializedDomainEvent)
    {
        _logger.LogInformation("Publishing domain event. Event - {Event}", fullName);

        var type = GetType(fullName);

        var domainEvent = DomainEvent.GetDomainEvent(type, serializedDomainEvent);

        return Handle(domainEvent);
    }

    private Task Handle(DomainEvent e)
    {
        var eventType    = e.GetType();
        var handlerType  = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<DomainEvent>.Handle));

        var handler = _serviceProvider.GetRequiredService(handlerType);

        return (Task) handleMethod!.Invoke(handler, new object[] { e, new CancellationToken(false) })!;
    }

    private static Type GetType(string fullName)
        => AppDomain.CurrentDomain.GetAssemblies().Select(c => c.GetType(fullName)).First(c => c is not null);
}
