using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Mesawer.InfrastructureLayer.Resources;
using Mesawer.ApplicationLayer;
using Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces;
using Mesawer.ApplicationLayer.Exceptions;
using Mesawer.ApplicationLayer.Extensions;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.DomainLayer.Entities;
using Mesawer.InfrastructureLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using BadRequestException = Mesawer.ApplicationLayer.Exceptions.BadRequestException;

namespace Mesawer.InfrastructureLayer.AspNetCore.Identity.Services;

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

    public async Task<bool> VerifyUserIdentityAsync(
        [NotNull] string userId,
        [NotNull] string token,
        CancellationToken ct)
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

    public async Task ValidateUserIdentityAsync(
        [NotNull] string userId,
        [NotNull] string token,
        CancellationToken ct)
    {
        var session = await GetOrCreateSession(userId, ct);

        await Validate(session, token, ct);
    }

    public async Task<(string token, string refreshToken)> UpdateUserSessionAsync(
        [NotNull] string userId,
        CancellationToken ct)
    {
        var session = await _context.Sessions.FindByKeyAsync(userId, ct);

        var token        = Guid.NewGuid().ToString("N");
        var refreshToken = GenerateRefreshToken();
        var mac          = GetUserMacAddress();

        if (session is null)
        {
            session = new TSession
            {
                UserId = userId
            };

            await _context.Sessions.AddAsync(session, ct);
        }

        session.Token        = token;
        session.RefreshToken = refreshToken;
        session.MacAddress   = mac is not null && Regex.IsMatch(mac, Regexes.Mac) ? mac : null;
        session.LastLogin    = _dateTime.Now;

        await _context.SaveChangesAsync(ct);

        return (token, refreshToken);
    }

    public async Task ResetUserSessionAsync([NotNull] string userId, bool forced, CancellationToken ct)
    {
        var session = await GetOrCreateSession(userId, ct);

        if (session is null) return;

        session.Token        = null;
        session.RefreshToken = null;

        if (forced) session.MacAddress = null;

        await _context.SaveChangesAsync(ct);
    }

    private async Task Validate(TSession session, string token, CancellationToken ct)
    {
        await ValidateSingleLogin(session, token, ct);
        await ValidateSingleDevice(session, ct);
    }

    private Task ValidateSingleLogin(TSession session, string token, CancellationToken ct)
    {
        if (!_identityOptions.RestrictSingleLogin) return Task.CompletedTask;

        if (token is null && !_identityOptions.LogoutOnNewLogin)
            throw new BadRequestException(SharedRes.AlreadyLoggedIn);

        // Checks if valid user
        if (session.Token != token) throw new ForbiddenAccessException();

        return Task.CompletedTask;
    }

    private Task ValidateSingleDevice(TSession session, CancellationToken ct)
    {
        if (!_identityOptions.RestrictSingleDevice) return Task.CompletedTask;

        var mac = GetUserMacAddress();

        if (mac is null || !Regex.IsMatch(mac, Regexes.Mac))
            throw new BadRequestException(SharedRes.UnauthorizedDevice);

        if (session.MacAddress != mac) throw new BadRequestException(SharedRes.UnauthorizedDevice);

        return Task.CompletedTask;
    }

    private async Task<TSession> GetOrCreateSession(string userId, CancellationToken ct)
    {
        var session = await _context.Sessions.FindByKeyAsync(userId, ct);

        if (session is not null) return session;

        var mac = GetUserMacAddress();

        session = new TSession
        {
            UserId     = userId,
            MacAddress = mac is not null && Regex.IsMatch(mac, Regexes.Mac) ? mac : null,
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
