using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Hangfire.Dashboard;
using Mesawer.ApplicationLayer;
using Mesawer.ApplicationLayer.Extensions;
using Mesawer.InfrastructureLayer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Serilog;

namespace Mesawer.PresentationLayer.Extensions
{
    public static class StartupExtensions
    {
        public static void UseSerilog(this IApplicationBuilder app)
            => app.UseSerilogRequestLogging(opts =>
            {
                opts.EnrichDiagnosticContext = (ctx, httpContext) =>
                {
                    ctx.Set(nameof(AppClaims.Id),
                        httpContext.User.Claim(AppClaims.Id)?.Value.ToLower() ??
                        "anonymous");

                    ctx.Set(nameof(AppClaims.UserName),
                        httpContext.User.Claim(AppClaims.UserName)?.Value.ToLower() ??
                        "anonymous");

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
                        return addresses[0].Contains(":")
                            ? addresses[0][..addresses[0].LastIndexOf(":", StringComparison.Ordinal)]
                            : addresses[0];

                    return "unknown";
                }
            });

        public static void UseAllStaticFiles(this IApplicationBuilder app) => app.UseStaticFiles();

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

        public static void MapEndpoints(
            this IApplicationBuilder app,
            Func<IEnumerable<IDashboardAuthorizationFilter>> hangfireAuthorizationFilters,
            bool useSpa = false,
            bool useProxy = false)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapHealthChecks("/api/Health");

                endpoints.MapHangfireDashboard(
                    "/api/BackgroundJobs",
                    new DashboardOptions
                    {
                        DashboardTitle = "Background Jobs",
                        Authorization  = hangfireAuthorizationFilters(),
                    }
                );
            });

            if (useSpa)
                app.UseSpa(spa =>
                {
                    // To learn more about options for serving an Angular SPA from ASP.NET Core,
                    // see https://go.microsoft.com/fwlink/?linkid=864501
                    spa.Options.SourcePath = "ClientApp";

                    spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
                    {
                        OnPrepareResponse = context =>
                        {
                            var headers = context.Context.Response.Headers;

                            headers[HeaderNames.CacheControl] =
                                "no-cache, no-store, must-revalidate"; // HTTP 1.1

                            headers[HeaderNames.Pragma]  = "no-cache"; // HTTP 1.0
                            headers[HeaderNames.Expires] = "-1";       // Proxies
                            headers.Remove(HeaderNames.ETag);          // Proxies
                        }
                    };

                    if (useProxy)
                        spa.UseProxyToSpaDevelopmentServer(
                            Environment.GetEnvironmentVariable(Constants.SpaProxyBaseUrlName));
                });
        }
    }
}
