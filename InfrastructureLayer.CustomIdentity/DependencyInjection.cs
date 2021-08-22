using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.CustomIdentity.Interfaces;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.DomainLayer.CustomIdentity.Entities;
using Mesawer.DomainLayer.Entities;
using Mesawer.InfrastructureLayer.CustomIdentity.Persistence;
using Mesawer.InfrastructureLayer.CustomIdentity.Services;
using Mesawer.InfrastructureLayer.Interfaces;
using Mesawer.InfrastructureLayer.Models;
using Mesawer.InfrastructureLayer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Mesawer.InfrastructureLayer.CustomIdentity
{
    public static class DependencyInjection
    {
        public static void ConfigureDatabase<TIDbContext, TContext, TUser, TAccount, TSession, TRole>(
            this IInfrastructureServiceCollection infraCollection)
            where TIDbContext : class, IIdentityDbContext<TUser, TAccount, TSession, TRole>
            where TContext : ApplicationDbContext<TUser, TAccount, TSession, TRole>, TIDbContext
            where TUser : ApplicationUser
            where TAccount : Account<TUser, TRole>
            where TSession : Session, new()
            where TRole : Enum
        {
            infraCollection.Services.AddDbContext<TContext>(
                ops =>
                {
                    ops.UseSqlServer(
                        infraCollection.Options.ConnectionString,
                        o => o.MigrationsAssembly(typeof(TContext).Assembly.FullName));

                    if (infraCollection.Options.IsDevelopment) ops.EnableSensitiveDataLogging();
                });

            infraCollection.Services.AddScoped<IDbContext<TSession>, TContext>();
            infraCollection.Services.AddScoped<IIdentityDbContext<TUser, TAccount, TSession, TRole>, TContext>();
            infraCollection.Services.AddScoped<TIDbContext, TContext>();

            infraCollection.Services.AddHealthChecks()
                .AddDbContextCheck<TContext>();

            infraCollection.Services.ConfigureIdentity<TUser, TAccount, TSession, TRole>(infraCollection.Options);
        }

        private static void ConfigureIdentity<TUser, TAccount, TSession, TRole>(
            this IServiceCollection services,
            InfrastructureOptions options)
            where TUser : ApplicationUser
            where TAccount : Account<TUser, TRole>
            where TSession : Session, new()
            where TRole : Enum
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

            services.AddTransient<ITokenService<TUser>, TokenService<TUser, TRole>>();

            services.AddScoped<ITokenGeneratorService<TUser>, TokenGeneratorService<TUser>>();
            services.AddScoped<ITokenValidatorService, TokenValidatorService>();
            services.AddScoped<IUserValidatorService, UserValidatorService<TUser, TAccount, TSession, TRole>>();
            services.AddScoped<IIdentityManager<TUser, TRole>, IdentityManager<TUser, TRole>>();

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
