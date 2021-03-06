using System;
using System.Linq;
using System.Text.RegularExpressions;
using Mesawer.ApplicationLayer;
using Mesawer.ApplicationLayer.AspNetCore.Identity.Exceptions;
using Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces;
using Mesawer.ApplicationLayer.Enums;
using Mesawer.ApplicationLayer.Extensions;
using Mesawer.DomainLayer.AspNetCore.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using IdentityOptions = Mesawer.InfrastructureLayer.Models.IdentityOptions;

namespace Mesawer.InfrastructureLayer.AspNetCore.Identity.Services;

public class IdentityManager<TUser> : IIdentityManager<TUser> where TUser : ApplicationUser
{
    private readonly IdentityOptions _identityOptions;

    public IdentityManager(
        SignInManager<TUser> singInManager,
        UserManager<TUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenGeneratorService<TUser> tokenGenerator,
        ITokenValidatorService tokenValidator,
        IUserValidatorService userValidator,
        IOptions<IdentityOptions> identityOptions)
    {
        SignInManager    = singInManager;
        UserManager      = userManager;
        RoleManager      = roleManager;
        TokenGenerator   = tokenGenerator;
        TokenValidator   = tokenValidator;
        UserValidator    = userValidator;
        _identityOptions = identityOptions.Value;
    }

    public SignInManager<TUser> SignInManager { get; }
    public UserManager<TUser> UserManager { get; }
    public RoleManager<IdentityRole> RoleManager { get; }
    public ITokenGeneratorService<TUser> TokenGenerator { get; }
    public ITokenValidatorService TokenValidator { get; }
    public IUserValidatorService UserValidator { get; }

    public string GetPhoneNumber(string phoneNumber)
    {
        var acceptedCodes = _identityOptions.AcceptedCodes is not null && _identityOptions.AcceptedCodes.Any()
            ? _identityOptions.AcceptedCodes
            : EnumExtensions.ToArray<PhoneNumberCode>();

        var codes = acceptedCodes
            .Select(c => (int) c)
            .Select(c => (c / 100, c % 100));

        var match = Regex.Match(phoneNumber, Regexes.PhoneNumber);

        if (!match.Success) throw new BadRequestException("This country's phone numbers aren't supported");

        var code   = Convert.ToInt32(match.Groups[1].ToString());
        var number = match.Groups[2].ToString();

        return codes.Any(c => c.Item1 == code && number.Length == c.Item2)
            ? phoneNumber
            : throw new BadRequestException("This country's phone numbers aren't supported");
    }
}
