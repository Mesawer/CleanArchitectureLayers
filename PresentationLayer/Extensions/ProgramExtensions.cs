using System;
using System.IO;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.DomainLayer.Entities;
using Mesawer.InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;

namespace Mesawer.PresentationLayer.Extensions;

public static class ProgramExtensions
{
    public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder)
    {
        var env = Environment.GetEnvironmentVariable(Constants.EnvironmentVariableName) ?? "Production";

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{env}.json", true)
            .AddEnvironmentVariables()
            .Build();

        Log.Logger = new LoggerConfiguration()
            .Enrich.WithProperty("Version", config["Version"])
            .Enrich.WithExceptionDetails()
            .ReadFrom.Configuration(config)
            .CreateLogger();

        Log.Information("::: Logging Started :::");

        return hostBuilder.UseSerilog();
    }

    public static async Task<IHost> MigrateDatabase<TProgram, TContext>(
        this IHost host,
        Func<IServiceProvider, Task> seedFunc)
        where TContext : DbContext
    {
        using var scope    = host.Services.CreateScope();
        var       services = scope.ServiceProvider;

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TProgram>>();

        try
        {
            var context = services.GetRequiredService<TContext>();

            if (context.Database.IsSqlServer())
            {
                logger.LogInformation("-- Migrating Sql Server Database --");

                await context.Database.MigrateAsync();
            }

            await seedFunc(services);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error occurred while migrating or seeding the database.");

            throw;
        }

        return host;
    }

    public static async Task RunAsync<TProgram>(this Task<IHost> hostTask)
    {
        var host = await hostTask;

        using var scope = host.Services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TProgram>>();

        try
        {
            await host.RunAsync();
            Log.CloseAndFlush();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error occurred while running the application.");

            throw;
        }
    }
}
