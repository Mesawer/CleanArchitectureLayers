using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.DomainLayer.AspNetCore.Identity.Entities;
using Mesawer.DomainLayer.Entities;
using Mesawer.InfrastructureLayer.AspNetCore.Identity.Persistence;
using Mesawer.InfrastructureLayer.AspNetCore.Identity.Services;
using Mesawer.InfrastructureLayer.Models;
using Mesawer.InfrastructureLayer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using LocalizedIdentityErrorDescriber = Mesawer.InfrastructureLayer.AspNetCore.Identity.Services.LocalizedIdentityErrorDescriber;

namespace Mesawer.InfrastructureLayer.AspNetCore.Identity
{
    public static class DependencyInjection
    {
        public static void ConfigureDatabase<TIDbContext, TContext, TUser, TAccount, TSession>(
            this IServiceCollection services,
            InfrastructureOptions options)
            where TIDbContext : class, IIdentityDbContext<TUser, TAccount, TSession>
            where TContext : IdentityDbContext<TUser, TAccount, TSession>, TIDbContext
            where TUser : ApplicationUser
            where TAccount : Account<TUser, TSession>
            where TSession : Session, new()
        {
            services.AddDbContext<TContext>(
                ops =>
                {
                    ops.UseSqlServer(
                        options.ConnectionString,
                        o => o.MigrationsAssembly(typeof(TContext).Assembly.FullName));

                    if (options.IsDevelopment) ops.EnableSensitiveDataLogging();
                });

            services.AddScoped<IIdentityDbContext<TUser, TAccount, TSession>, TContext>();
            services.AddScoped<TIDbContext, TContext>();

            services.AddHealthChecks()
                .AddDbContextCheck<TContext>();

            services.ConfigureIdentity<TContext, TUser, TAccount, TSession>(options);
        }

        public static void ConfigureIdentity<TContext, TUser, TAccount, TSession>(
            this IServiceCollection services,
            InfrastructureOptions options)
            where TContext : IdentityDbContext<TUser, TAccount, TSession>
            where TUser : ApplicationUser
            where TAccount : Account<TUser, TSession>
            where TSession : Session, new()
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddIdentity<TUser, IdentityRole>(opts =>
                {
                    opts.ClaimsIdentity.UserIdClaimType     = AppClaims.Id;
                    opts.ClaimsIdentity.RoleClaimType       = AppClaims.Roles;
                    opts.ClaimsIdentity.UserNameClaimType   = AppClaims.UserName;
                    opts.SignIn.RequireConfirmedEmail       = options.IdentityOptions.RequireConfirmedEmail;
                    opts.SignIn.RequireConfirmedPhoneNumber = options.IdentityOptions.RequireConfirmedPhoneNumber;
                    opts.User.RequireUniqueEmail            = options.IdentityOptions.RequireUniqueEmail;
                    opts.Password.RequiredLength            = options.IdentityOptions.Password.RequiredLength;
                    opts.Password.RequireNonAlphanumeric    = options.IdentityOptions.Password.RequireNonAlphanumeric;
                    opts.Password.RequireLowercase          = options.IdentityOptions.Password.RequireLowercase;
                    opts.Password.RequireUppercase          = options.IdentityOptions.Password.RequireUppercase;
                    opts.Password.RequireDigit              = options.IdentityOptions.Password.RequireDigit;
                    opts.Lockout.MaxFailedAccessAttempts    = 5;
                    opts.Lockout.DefaultLockoutTimeSpan     = TimeSpan.FromMinutes(5);
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

            services.AddTransient<ITokenService<TUser>, TokenService<TUser, TAccount, TSession>>();

            services.AddScoped<ITokenGeneratorService<TUser>, Services.TokenGeneratorService<TUser>>();
            services.AddScoped<ITokenValidatorService, Services.TokenValidatorService>();
            services.AddScoped<IUserValidatorService, UserValidatorService<TSession>>();
            services
                .AddScoped<IIdentityManager<TUser, TAccount, TSession>, IdentityManager<TUser, TAccount, TSession>>();

            services.AddSingleton<IApplicationUserService, ApplicationUserService>();

            services.Configure<Models.IdentityOptions>(config =>
            {
                config.RestrictSingleLogin      = options.IdentityOptions.RestrictSingleLogin;
                config.LogoutOnNewLogin         = options.IdentityOptions.LogoutOnNewLogin;
                config.RestrictSingleDevice     = options.IdentityOptions.RestrictSingleDevice;
                config.NumericVerificationToken = options.IdentityOptions.NumericVerificationToken;
                config.NumericTokenLength       = options.IdentityOptions.NumericTokenLength;
                config.SessionExpirationPeriod  = options.IdentityOptions.SessionExpirationPeriod;
                config.TokenExpirationPeriod    = options.IdentityOptions.TokenExpirationPeriod;
                config.TokenResetPeriod         = options.IdentityOptions.TokenResetPeriod;
                config.AcceptedCodes            = options.IdentityOptions.AcceptedCodes;
            });

            services.Configure<JwtConfig>(config =>
            {
                config.Key              = options.IdentityOptions.JwtConfig.Key;
                config.Issuer           = options.IdentityOptions.JwtConfig.Issuer;
                config.ExpirationPeriod = options.IdentityOptions.JwtConfig.ExpirationPeriod;
            });
        }
    }
}
