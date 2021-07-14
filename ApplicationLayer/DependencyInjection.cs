using System.Reflection;
using FluentValidation;
using MediatR;
using MediatR.Pipeline;
using Mesawer.ApplicationLayer.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Mesawer.ApplicationLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureApplicationLayer(
            this IServiceCollection services,
            Assembly validationsExecutingAssembly,
            Assembly mediatRExecutingAssembly)
        {
            services.AddValidatorsFromAssembly(validationsExecutingAssembly);
            services.ConfigureMediatR(mediatRExecutingAssembly);

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
    }
}
