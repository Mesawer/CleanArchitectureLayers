using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Mesawer.PresentationLayer.Extensions;

[PublicAPI]
public static class StartupExtensions
{
    public static void UseSerilog(this IApplicationBuilder app)
        => app.UseSerilogRequestLogging(opts =>
        {
            opts.EnrichDiagnosticContext = (ctx, httpContext) =>
            {
                ctx.Set("UserAgent",
                    httpContext.Request.Headers["User-Agent"].FirstOrDefault());

                // The IP address of the client from which a request originated
                ctx.Set("ClientIp", GetIpAddress(httpContext));
                // Get name of the current endpoint. Log this with requests help if need to aggregate all
                // requests for a particular endpoint in the application.
                // The Serilog middleware by default logs the request path but it will be a
                // challenge if the path contains dynamic parameters.
                ctx.Set("Resource", httpContext.GetEndpoint()?.DisplayName);
            };

            static string GetIpAddress(HttpContext httpContext)
            {
                var ipAddress = httpContext.Request.Headers["X-forwarded-for"].FirstOrDefault();

                // Is not proxy
                if (string.IsNullOrEmpty(ipAddress))
                    return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                // Is proxy

                var addresses = ipAddress.Split(',');

                if (addresses.Length != 0)
                    // If IP contains port, it will be after the last : (IPv6 uses : as delimiter and could have more of them)
                    return addresses[0].Contains(':')
                        ? addresses[0][..addresses[0].LastIndexOf(":", StringComparison.Ordinal)]
                        : addresses[0];

                return "unknown";
            }
        });

    public static void UseApiDocs(this IApplicationBuilder app)
    {
        app.UseOpenApi();

        app.UseReDoc(config =>
        {
            config.Path         = "/docs/re-doc";
            config.DocumentPath = "/specification.json";
        });

        app.UseSwaggerUi3(config =>
        {
            config.Path         = "/docs/swagger";
            config.DocumentPath = "/specification.json";
        });
    }
}
