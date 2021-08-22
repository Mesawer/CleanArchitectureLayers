using System;
using System.Linq;
using System.Text.RegularExpressions;
using Mesawer.ApplicationLayer;
using Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces;
using Mesawer.DomainLayer.AspNetCore.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using IdentityOptions = Mesawer.InfrastructureLayer.Models.IdentityOptions;

namespace Mesawer.InfrastructureLayer.AspNetCore.Identity.Services
{
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
            if (_identityOptions.AcceptedCodes is null || !_identityOptions.AcceptedCodes.Any()) return phoneNumber;

            var codes = _identityOptions.AcceptedCodes
                .Select(c => (int) c)
                .Select(c => (c / 100, c % 100));

            var match = Regex.Match(phoneNumber, Regexes.PhoneNumber);

            if (!match.Success) return null;

            var code   = Convert.ToInt32(match.Groups[1].ToString());
            var number = match.Groups[2].ToString();

            return codes.Any(c => c.Item1 == code && number.Length == c.Item2) ? phoneNumber : null;
        }
    }
}
