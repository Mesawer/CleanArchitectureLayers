using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Mesawer.InfrastructureLayer.Resources;
using Mesawer.ApplicationLayer;
using Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces;
using Mesawer.ApplicationLayer.Extensions;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.DomainLayer.Entities;
using Mesawer.InfrastructureLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Errors.Model;
using BadRequestException = Mesawer.ApplicationLayer.Exceptions.BadRequestException;

namespace Mesawer.InfrastructureLayer.AspNetCore.Identity.Services
{
    public class UserValidatorService<TSession> : IUserValidatorService
        where TSession : Session, new()
    {
        private readonly IDbContext<TSession> _context;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IDateTime            _dateTime;
        private readonly IdentityOptions      _identityOptions;

        public UserValidatorService(
            IDbContext<TSession> context,
            IHttpContextAccessor httpContext,
            IOptions<IdentityOptions> userIdentityOptions,
            IDateTime dateTime)
        {
            _context         = context;
            _httpContext     = httpContext;
            _dateTime        = dateTime;
            _identityOptions = userIdentityOptions.Value;
        }

        /// <summary>
        /// Validates user identity using a certain session <paramref name="token"/>
        /// </summary>
        /// <param name="userId">User's Id</param>
        /// <param name="token">Session token, or null to reset session</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>A boolean indicating whether valid or not</returns>
        /// <remarks>Thread Safe</remarks>
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

        /// <summary>
        /// Validates user identity using a certain session <paramref name="token"/>
        /// </summary>
        /// <param name="userId">User's Id</param>
        /// <param name="token">Session token, or null to reset session</param>
        /// <param name="ct">Cancellation Token</param>
        /// <exception cref="UnauthorizedException"></exception>
        /// <exception cref="BadRequestException"></exception>
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

            if (session is null)
            {
                var mac = GetUserMacAddress();

                session = new TSession
                {
                    UserId     = userId,
                    MacAddress = Regex.IsMatch(mac, Regexes.Mac) ? mac : null,
                };

                await _context.Sessions.AddAsync(session, ct);
            }

            session.Token        = token;
            session.RefreshToken = refreshToken;
            session.LastLogin    = _dateTime.Now;

            await _context.SaveChangesAsync(ct);

            return (token, refreshToken);
        }

        /// <summary>
        /// Resets User Session if exists
        /// </summary>
        /// <param name="userId">User's Id</param>
        /// <param name="forced">true to reset the MacAddress</param>
        /// <param name="ct">Cancellation Token</param>
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
                UserId     = userId,
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
