using Mesawer.DomainLayer.AspNetCore.Identity.Entities;
using Mesawer.DomainLayer.Entities;
using Microsoft.AspNetCore.Identity;

namespace Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces
{
    public interface IIdentityManager<TUser, TAccount, TSession>
        where TUser : ApplicationUser where TAccount : Account<TUser, TSession> where TSession : Session
    {
        public SignInManager<TUser> SignInManager { get; }
        public UserManager<TUser> UserManager { get; }
        public RoleManager<TAccount> RoleManager { get; }
        public ITokenGeneratorService<TUser> TokenGenerator { get; }
        public ITokenValidatorService TokenValidator { get; }
        public IUserValidatorService UserValidator { get; }

        public string GetPhoneNumber(string phoneNumber);
    }
}
