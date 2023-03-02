using System;
using Hangfire;
using Hangfire.SqlServer;
using JetBrains.Annotations;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.InfrastructureLayer.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Mesawer.InfrastructureLayer;

[PublicAPI]
public static class DependencyInjection
{
    public static void ConfigureHangfire(this IServiceCollection services, HangfireOptions options)
    {
        if (!options.UseHangfire)
        {
            services.AddScoped<IBackgroundJobService, DevBackgroundJobService>();
            return;
        }

        services.AddHangfire(config =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseSerializerSettings(new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            if (!options.UseInMemoryDatabase)
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

                config.UseSqlServerStorage(options.ConnectionString, storageOptions);
            }
            else
            {
                config.UseInMemoryStorage();
            }
        });

        // Add the processing server as IHostedService
        services.AddHangfireServer();

        services.AddTransient<IBackgroundJobService, BackgroundJobService>();
    }

    /// <summary>
    /// Configure Application Services (e.g. IMemoryCacheService, IDomainEventService, IDateTime)
    /// </summary>
    public static void ConfigureInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IMemoryCacheService, MemoryCacheService>();
        services.AddScoped<IDomainEventService, DomainEventService>();

        services.AddTransient<IDateTime, DateTimeService>();
    }
}
