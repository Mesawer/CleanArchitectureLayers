using System;
using Mesawer.DomainLayer.CustomIdentity.Entities;

namespace Mesawer.ApplicationLayer.CustomIdentity.Interfaces
{
    public interface IIdentityManager<TUser, TRole> where TUser : ApplicationUser where TRole : Enum
    {
        public IUserFinderService<TUser> UserFinder { get; }
        public IUserManager<TUser, TRole> UserManager { get; }
        public ISignInManager<TUser> SignInManager { get; }
        public ITokenGeneratorService<TUser> TokenGenerator { get; }
        public ITokenValidatorService TokenValidator { get; }
        public IUserValidatorService UserValidator { get; }

        public string GetPhoneNumber(string phoneNumber);
    }
}
