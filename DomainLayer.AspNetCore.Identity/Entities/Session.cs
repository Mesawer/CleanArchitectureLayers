using System;
using System.Security.Cryptography;
using JetBrains.Annotations;

namespace Mesawer.DomainLayer.AspNetCore.Identity.Entities;

[PublicAPI]
public class Session
{
    public string UserId { get; init; }
    public User User { get; set; }

    public string Token { get; private set; } = Guid.NewGuid().ToString("N");

    public string RefreshToken { get; private set; } = GenerateRefreshToken();

    public string MacAddress { get; set; }

    public void Reset()
    {
        Token        = Guid.NewGuid().ToString("N");
        RefreshToken = GenerateRefreshToken();
    }

    private static string GenerateRefreshToken()
    {
        using var rngCryptoServiceProvider = RandomNumberGenerator.Create();

        var randomBytes = new byte[64];
        rngCryptoServiceProvider.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }
}
