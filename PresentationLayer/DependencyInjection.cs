using System;
using System.Linq;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using Mesawer.ApplicationLayer.Extensions;
using Mesawer.PresentationLayer.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace Mesawer.PresentationLayer;

public static class DependencyInjection
{
    public static IServiceCollection ConfigurePresentationLayer(
        this IServiceCollection services,
        Action<PresentationOptions> configureOptions)
    {
        var options = new PresentationOptions();

        configureOptions(options);

        services.AddCors(options);
        services.ConfigureLocalization();
        services.ConfigureDocs(options);
        services.ConfigureMvcApi();

        return services;
    }

    private static void AddCors(this IServiceCollection services, PresentationOptions options)
        => services.AddCors(ops => ops.AddPolicy(options.CorsPolicy,
            builder =>
            {
                if (!options.IsProduction)
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    return;
                }

                options.CorsOrigins?
                    .Select(o => o.Value)
                    .Where(value => !string.IsNullOrEmpty(value))
                    .ForEach(value => builder
                        .AllowAnyMethod().AllowAnyHeader()
                        .WithOrigins(value)
                        .AllowCredentials());
            }));

    public static void ConfigureLocalization(this IServiceCollection services)
    {
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                "en", "ar"
            };
            options.SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
        });

        services.AddLocalization();
    }

    private static void ConfigureDocs(
        this IServiceCollection services,
        PresentationOptions options)
    {
        //services.AddSingleton<FluentValidationSchemaProcessor>();

        if (!options.IsProduction)
            services.AddOpenApiDocument((configure, serviceProvider) =>
            {
                configure.Title        = options.ApiTitle ?? "API";
                configure.Version      = options.Version;
                configure.DocumentName = "specification";
                configure.Description  = ApiDocs.Description;

                configure.AddSecurity("JWT",
                    Enumerable.Empty<string>(),
                    new OpenApiSecurityScheme
                    {
                        Type        = OpenApiSecuritySchemeType.ApiKey,
                        Name        = "Authorization",
                        In          = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Type into the TextBox: Bearer {your JWT token}."
                    });

                configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));

                configure.GenerateEnumMappingDescription = true;

                // Add the fluent validations schema processor
                //var fluentValidationSchemaProcessor = serviceProvider.GetService<FluentValidationSchemaProcessor>();
                //configure.SchemaProcessors.Add(fluentValidationSchemaProcessor);
            });
    }

    private static void ConfigureMvcApi(this IServiceCollection services)
    {
        services.AddControllersWithViews(options => options.Filters.Add<ApiExceptionFilterAttribute>())
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            })
            .AddJsonOptions(options
                => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)
            .AddFluentValidation()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();

        services.AddRazorPages();

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
    }
}
