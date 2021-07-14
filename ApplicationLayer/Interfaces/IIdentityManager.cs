using Mesawer.ApplicationLayer.Models;
using Microsoft.AspNetCore.Identity;

namespace Mesawer.ApplicationLayer.Interfaces
{
    public interface IIdentityManager<TUser> where TUser : ApplicationUser
    {
        public SignInManager<TUser> SignInManager { get; }
        public UserManager<TUser> UserManager { get; }
        public RoleManager<IdentityRole> RoleManager { get; }
        public ITokenGeneratorService<TUser> TokenGenerator { get; }
        public ITokenValidatorService TokenValidator { get; }
        public IUserValidatorService UserValidator { get; }
    }
}
