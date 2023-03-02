using System.Linq;
using System.Reflection;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using MediatR.Pipeline;
using Mesawer.ApplicationLayer.Behaviors;
using Mesawer.ApplicationLayer.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mesawer.ApplicationLayer;

[PublicAPI]
public static class DependencyInjection
{
    /// <summary>
    /// Configures MediatR and its Behaviors, Validators, and EventHandlers
    /// </summary>
    public static IServiceCollection ConfigureApplicationLayer(this IServiceCollection services, Assembly executingAssembly)
    {
        services.AddValidatorsFromAssembly(executingAssembly);
        services.ConfigureMediatR(executingAssembly);
        services.ConfigureEventHandlers(executingAssembly);

        return services;
    }

    private static void ConfigureMediatR(this IServiceCollection services, Assembly mediatRExecutingAssembly)
    {
        services.AddMediatR(mediatRExecutingAssembly);

        services.AddTransient(typeof(IRequestPostProcessor<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
    }

    private static void ConfigureEventHandlers(this IServiceCollection services, Assembly executingAssembly)
    {
        var types = executingAssembly
            .DefinedTypes
            .Select(c => (
                BaseType:
                c.FindInterfaces((t, _)
                            => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>),
                        null)
                    .FirstOrDefault(),
                Type: c))
            .Where(c => c.BaseType is not null)
            .ToList();

        foreach (var type in types)
        {
            services.AddScoped(type.BaseType!, type.Type);
        }
    }
}
