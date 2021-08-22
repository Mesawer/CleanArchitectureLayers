using System;
using Hangfire;
using Hangfire.SqlServer;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.InfrastructureLayer.Interfaces;
using Mesawer.InfrastructureLayer.Models;
using Mesawer.InfrastructureLayer.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Mesawer.InfrastructureLayer
{
    public static class DependencyInjection
    {
        public static IInfrastructureServiceCollection ConfigureInfrastructureLayer(
            this IServiceCollection services,
            Action<InfrastructureOptions> configureOptions)
        {
            var options = new InfrastructureOptions();

            configureOptions(options);

            services.ConfigureHangfire(options.ConnectionString);
            services.ConfigureServices(options);
            services.ConfigureEmails(options);

            return new InfrastructureServiceCollection
            {
                Services = services,
                Options  = options
            };
        }

        private static void ConfigureHangfire(this IServiceCollection services, string connection)
        {
            var storageOptions = new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout       = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout   = TimeSpan.FromMinutes(5),
                QueuePollInterval            = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                UsePageLocksOnDequeue        = true,
                DisableGlobalLocks           = true,
            };

            // Add Hangfire services.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseSerializerSettings(new JsonSerializerSettings
                    { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                .UseSqlServerStorage(connection, storageOptions));

            // Add the processing server as IHostedService
            services.AddHangfireServer();
        }

        private static void ConfigureServices(this IServiceCollection services, InfrastructureOptions options)
        {
            services.AddScoped<DomainEventService>();
            services.AddScoped<IMemoryCacheService, MemoryCacheService>();
            services.AddScoped<IRazorRendererService, RazorRendererService>();

            services.AddScoped(typeof(IDomainEventService), options.DomainEventService);

            services.AddScoped(typeof(IStorageLocationService), options.StorageLocationService);

            services.AddScoped(typeof(IStorageService), options.StorageService);

            services.AddTransient<IBackgroundJobService, BackgroundJobService>();
            services.AddTransient<IDateTime, DateTimeService>();

            if (options.IsProduction)
                services.AddApplicationInsightsTelemetry(
                    ops =>
                    {
                        ops.DeveloperMode      = false;
                        ops.ApplicationVersion = options.Version;
                        ops.InstrumentationKey = options.ApplicationInsightsConfig?.InstrumentationKey;
                    });
        }

        private static void ConfigureEmails(this IServiceCollection services, InfrastructureOptions options)
        {
            services.Configure<SendGridConfig>(config =>
            {
                config.ApiKey    = options.SendGridConfig?.ApiKey;
                config.FromEmail = options.SendGridConfig?.FromEmail;
                config.FromName  = options.SendGridConfig?.FromName;
            });

            services.AddScoped(typeof(IEmailSender), options.EmailService);
        }
    }
}
