using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.ApplicationLayer.Models;
using Mesawer.DomainLayer.Entities;
using Mesawer.InfrastructureLayer.Identity;
using Mesawer.InfrastructureLayer.Models;
using Mesawer.InfrastructureLayer.Persistence;
using Mesawer.InfrastructureLayer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using IdentityOptions = Mesawer.InfrastructureLayer.Models.IdentityOptions;

namespace Mesawer.InfrastructureLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureInfrastructureLayer<TIDbContext, TContext, TUser, TSession>(
            this IServiceCollection services,
            Action<InfrastructureOptions> configureOptions)
            where TIDbContext : class, IDbContext<TSession>
            where TContext : ApplicationDbContext<TUser, TSession>, TIDbContext
            where TUser : ApplicationUser
            where TSession : Session, new()
        {
            var options = new InfrastructureOptions();

            configureOptions(options);

            services.ConfigureDatabase<TIDbContext, TContext, TUser, TSession>(options);
            services.ConfigureHangfire(options.ConnectionString);
            services.ConfigureIdentity<TContext, TUser, TSession>(options);
            services.ConfigureServices(options);
            services.ConfigureEmails(options);

            return services;
        }

        private static void ConfigureDatabase<TIDbContext, TContext, TUser, TSession>(
            this IServiceCollection services,
            InfrastructureOptions options)
            where TIDbContext : class, IDbContext<TSession>
            where TContext : ApplicationDbContext<TUser, TSession>, TIDbContext
            where TUser : ApplicationUser
            where TSession : Session
        {
            services.AddDbContext<TContext>(
                ops =>
                {
                    ops.UseSqlServer(
                        options.ConnectionString,
                        o => o.MigrationsAssembly(typeof(TContext).Assembly.FullName));

                    if (options.IsDevelopment) ops.EnableSensitiveDataLogging();
                });

            services.AddScoped<IDbContext<TSession>, TContext>();
            services.AddScoped<TIDbContext, TContext>();

            services.AddHealthChecks()
                .AddDbContextCheck<TContext>();
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

        public static void ConfigureIdentity<TContext, TUser, TSession>(
            this IServiceCollection services,
            InfrastructureOptions options)
            where TContext : ApplicationDbContext<TUser, TSession>
            where TUser : ApplicationUser
            where TSession : Session, new()
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddIdentity<TUser, IdentityRole>(opts =>
                {
                    opts.ClaimsIdentity.UserIdClaimType   = AppClaims.Id;
                    opts.ClaimsIdentity.RoleClaimType     = AppClaims.Roles;
                    opts.ClaimsIdentity.UserNameClaimType = AppClaims.UserName;
                    opts.SignIn.RequireConfirmedAccount   = true;
                    opts.User.RequireUniqueEmail          = options.IdentityOptions.RequireUniqueEmail;
                    opts.Password.RequiredLength          = options.IdentityOptions.Password.RequiredLength;
                    opts.Password.RequireNonAlphanumeric  = options.IdentityOptions.Password.RequireNonAlphanumeric;
                    opts.Password.RequireLowercase        = options.IdentityOptions.Password.RequireLowercase;
                    opts.Password.RequireUppercase        = options.IdentityOptions.Password.RequireUppercase;
                    opts.Password.RequireDigit            = options.IdentityOptions.Password.RequireDigit;
                    opts.Lockout.MaxFailedAccessAttempts  = 5;
                    opts.Lockout.DefaultLockoutTimeSpan   = TimeSpan.FromMinutes(5);
                })
                .AddRoles<IdentityRole>()
                .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
                .AddEntityFrameworkStores<TContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(
                    opts =>
                    {
                        opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        opts.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
                        opts.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(
                    opts =>
                    {
                        opts.RequireHttpsMetadata = false;
                        opts.SaveToken            = true;

                        opts.TokenValidationParameters = new TokenValidationParameters
                        {
                            NameClaimType            = AppClaims.UserName,
                            RoleClaimType            = AppClaims.Roles,
                            ValidateIssuer           = options.IdentityOptions.JwtConfig.Issuer is not null,
                            ValidIssuer              = options.IdentityOptions.JwtConfig.Issuer,
                            ValidateAudience         = false,
                            ValidAudience            = options.IdentityOptions.JwtConfig.Issuer,
                            ClockSkew                = TimeSpan.Zero, // remove delay of token when expire
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                                options.IdentityOptions.JwtConfig.Key
                                ?? throw new InvalidOperationException("Jwt key can not be null"))),
                        };

                        opts.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Query["access_token"];

                                var path = context.HttpContext.Request.Path;

                                if (!string.IsNullOrEmpty(accessToken) &&
                                    path.StartsWithSegments("/Hub", StringComparison.OrdinalIgnoreCase))
                                    context.Token = accessToken; // Read the token out of the query string

                                return Task.CompletedTask;
                            },
                        };
                    });

            services.AddAuthorization();

            services.AddTransient<ITokenService<TUser>, TokenService<TUser>>();

            services.AddScoped<ITokenGeneratorService<TUser>, TokenGeneratorService<TUser>>();
            services.AddScoped<ITokenValidatorService, TokenValidatorService>();
            services.AddScoped<IUserValidatorService, UserValidatorService<TSession>>();
            services.AddScoped<IIdentityManager<TUser>, IdentityManager<TUser>>();

            services.AddSingleton<IApplicationUserService, ApplicationUserService>();

            services.Configure<IdentityOptions>(config =>
            {
                config.RestrictSingleLogin      = options.IdentityOptions.RestrictSingleLogin;
                config.LogoutOnNewLogin         = options.IdentityOptions.LogoutOnNewLogin;
                config.RestrictSingleDevice     = options.IdentityOptions.RestrictSingleDevice;
                config.NumericVerificationToken = options.IdentityOptions.NumericVerificationToken;
                config.SessionExpirationPeriod  = options.IdentityOptions.SessionExpirationPeriod;
                config.TokenExpirationPeriod    = options.IdentityOptions.TokenExpirationPeriod;
            });

            services.Configure<JwtConfig>(config =>
            {
                config.Key              = options.IdentityOptions.JwtConfig.Key;
                config.Issuer           = options.IdentityOptions.JwtConfig.Issuer;
                config.ExpirationPeriod = options.IdentityOptions.JwtConfig.ExpirationPeriod;
            });
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
