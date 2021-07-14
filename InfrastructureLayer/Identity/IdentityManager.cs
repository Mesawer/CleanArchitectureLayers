using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.ApplicationLayer.Models;
using Microsoft.AspNetCore.Identity;

namespace Mesawer.InfrastructureLayer.Identity
{
    public class IdentityManager<TUser> : IIdentityManager<TUser> where TUser : ApplicationUser
    {
        public IdentityManager(
            SignInManager<TUser> singInManager,
            UserManager<TUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenGeneratorService<TUser> tokenGenerator,
            ITokenValidatorService tokenValidator,
            IUserValidatorService userValidator)
        {
            SignInManager  = singInManager;
            UserManager    = userManager;
            RoleManager    = roleManager;
            TokenGenerator = tokenGenerator;
            TokenValidator = tokenValidator;
            UserValidator  = userValidator;
        }

        public SignInManager<TUser> SignInManager { get; }
        public UserManager<TUser> UserManager { get; }
        public RoleManager<IdentityRole> RoleManager { get; }
        public ITokenGeneratorService<TUser> TokenGenerator { get; }
        public ITokenValidatorService TokenValidator { get; }
        public IUserValidatorService UserValidator { get; }
    }
}
