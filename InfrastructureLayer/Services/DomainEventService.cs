using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.DomainLayer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mesawer.InfrastructureLayer.Services
{
    public class DomainEventService : IDomainEventService
    {
        private readonly IServiceProvider            _serviceProvider;
        private readonly ILogger<DomainEventService> _logger;

        public DomainEventService(IServiceProvider serviceProvider, ILogger<DomainEventService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger          = logger;
        }

        public Task Publish(DomainEvent e, CancellationToken ct = default)
        {
            _logger.LogInformation("Publishing domain event. Event - {Event}", e.GetType().Name);
            return Handle(e, ct);
        }

        private Task Handle(DomainEvent e, CancellationToken ct)
        {
            var eventType    = e.GetType();
            var handlerType  = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<DomainEvent>.Handle));

            if (handleMethod is null) throw new InvalidOperationException("Could not find 'Handle' method implemented");

            var handlers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => handlerType.IsAssignableFrom(p))
                .ToList();

            if (!handlers.Any())
                throw new InvalidOperationException(
                    $"Could not find an implementation for the interface {handlerType}");

            if (handlers.Count > 1)
                throw new InvalidOperationException(
                    $"Could not determine which implementation to use for the interface {handlerType}");

            var instance = ActivatorUtilities.CreateInstance(_serviceProvider, handlers.First());

            return (Task) handleMethod.Invoke(instance, new object[] { e, ct });
        }
    }
}
