using System;
using System.Linq;
using System.Text.RegularExpressions;
using Mesawer.ApplicationLayer;
using Mesawer.ApplicationLayer.CustomIdentity.Interfaces;
using Mesawer.DomainLayer.CustomIdentity.Entities;
using Microsoft.Extensions.Options;
using IdentityOptions = Mesawer.InfrastructureLayer.Models.IdentityOptions;

namespace Mesawer.InfrastructureLayer.CustomIdentity.Services
{
    public class IdentityManager<TUser, TRole> : IIdentityManager<TUser, TRole>
        where TUser : ApplicationUser where TRole : Enum
    {
        private readonly IdentityOptions _identityOptions;

        public IdentityManager(
            IUserFinderService<TUser> userFinder,
            IUserManager<TUser, TRole> userManager,
            ISignInManager<TUser> signInManager,
            ITokenGeneratorService<TUser> tokenGenerator,
            ITokenValidatorService tokenValidator,
            IUserValidatorService userValidator,
            IOptions<IdentityOptions> identityOptions)
        {
            UserFinder       = userFinder;
            UserManager      = userManager;
            SignInManager    = signInManager;
            TokenGenerator   = tokenGenerator;
            TokenValidator   = tokenValidator;
            UserValidator    = userValidator;
            _identityOptions = identityOptions.Value;
        }

        public IUserFinderService<TUser> UserFinder { get; }
        public IUserManager<TUser, TRole> UserManager { get; }
        public ISignInManager<TUser> SignInManager { get; }
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
