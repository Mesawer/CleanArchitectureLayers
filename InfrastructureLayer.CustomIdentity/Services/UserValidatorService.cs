using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Mesawer.InfrastructureLayer.Resources;
using Mesawer.ApplicationLayer;
using Mesawer.ApplicationLayer.CustomIdentity.Interfaces;
using Mesawer.ApplicationLayer.Extensions;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.DomainLayer.CustomIdentity.Entities;
using Mesawer.DomainLayer.Entities;
using Mesawer.InfrastructureLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Errors.Model;
using BadRequestException = Mesawer.ApplicationLayer.Exceptions.BadRequestException;

namespace Mesawer.InfrastructureLayer.CustomIdentity.Services
{
    public class UserValidatorService<TUser, TAccount, TSession, TRole> : IUserValidatorService
        where TUser : ApplicationUser
        where TAccount : Account<TUser, TSession, TRole>
        where TSession : Session, new()
        where TRole : Enum
    {
        private readonly IIdentityDbContext<TUser, TAccount, TSession, TRole> _context;
        private readonly IHttpContextAccessor                                 _httpContext;
        private readonly IDateTime                                            _dateTime;
        private readonly IdentityOptions                                      _identityOptions;

        public UserValidatorService(
            IIdentityDbContext<TUser, TAccount, TSession, TRole> context,
            IHttpContextAccessor httpContext,
            IOptions<IdentityOptions> userIdentityOptions,
            IDateTime dateTime)
        {
            _context         = context;
            _httpContext     = httpContext;
            _dateTime        = dateTime;
            _identityOptions = userIdentityOptions.Value;
        }

        public async Task<bool> VerifyUserIdentityAsync(string userId, string token, CancellationToken ct)
        {
            var session = await GetOrCreateSession(userId, ct);

            try
            {
                await Validate(session, token, ct);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task ValidateUserIdentityAsync(string userId, string token, CancellationToken ct)
        {
            var session = await GetOrCreateSession(userId, ct);

            await Validate(session, token, ct);
        }

        public async Task<(string token, string refreshToken)> UpdateUserSessionAsync(
            string userId,
            CancellationToken ct)
        {
            var session = await _context.Sessions.FindByKeyAsync(userId, ct);

            var token        = Guid.NewGuid().ToString("N");
            var refreshToken = GenerateRefreshToken();

            if (session is null) return (token, refreshToken);

            session.Token        = token;
            session.RefreshToken = refreshToken;
            session.LastLogin    = _dateTime.Now;

            await _context.SaveChangesAsync(ct);

            return (token, refreshToken);
        }

        public async Task ResetUserSessionAsync(string userId, bool forced, CancellationToken ct)
        {
            var session = await _context.Sessions.FindByKeyAsync(userId, ct);

            if (session is null) return;

            session.Token        = null;
            session.RefreshToken = null;

            if (forced) session.MacAddress = null;

            await _context.SaveChangesAsync(ct);
        }

        private async Task Validate(TSession session, string token, CancellationToken ct)
        {
            await ValidateSingleLogin(session, token, ct);
            await ValidateSingleDevice(session, token is not null, ct);
        }

        private async Task ValidateSingleLogin(TSession session, string token, CancellationToken ct)
        {
            if (!_identityOptions.RestrictSingleLogin) return;

            // Checks if the user eligible for new token (login again)
            if (token is null)
            {
                var expirationDate = session.LastLogin.AddHours(_identityOptions.SessionExpirationPeriod);

                // Checks if there's no token in place, or if it's expired
                if (session.Token is null || expirationDate.IsOlderThan(_dateTime.Now)) return;

                if (!_identityOptions.LogoutOnNewLogin) throw new BadRequestException(SharedRes.AlreadyLoggedIn);

                session.Token        = null;
                session.RefreshToken = null;

                await _context.SaveChangesAsync(ct);
            }

            // Checks if valid user
            if (session.Token != token) throw new UnauthorizedException();
        }

        private async Task ValidateSingleDevice(TSession session, bool loggedIn, CancellationToken ct)
        {
            if (!_identityOptions.RestrictSingleDevice) return;

            var mac = GetUserMacAddress();

            if (!Regex.IsMatch(mac, Regexes.Mac)) throw new BadRequestException(SharedRes.UnauthorizedDevice);

            if (loggedIn)
            {
                if (session.MacAddress != mac) throw new BadRequestException(SharedRes.UnauthorizedDevice);
            }
            else
            {
                session.MacAddress = mac;

                await _context.SaveChangesAsync(ct);
            }
        }

        private async Task<TSession> GetOrCreateSession(string userId, CancellationToken ct)
        {
            var session = await _context.Sessions.FindByKeyAsync(userId, ct);

            if (session is not null) return session;

            var mac = GetUserMacAddress();

            session = new TSession
            {
                Id         = Guid.NewGuid().ToString("N"),
                MacAddress = Regex.IsMatch(mac, Regexes.Mac) ? mac : null,
                LastLogin  = _dateTime.Now
            };

            await _context.Sessions.AddAsync(session, ct);
            await _context.SaveChangesAsync(ct);

            return session;
        }

        private string GetUserMacAddress() => _httpContext.HttpContext?.Request.Headers["Mac-Address"].ToString();

        private static string GenerateRefreshToken()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();

            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
