using Mesawer.DomainLayer.AspNetCore.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces;

public interface IIdentityManager<TUser> where TUser : ApplicationUser
{
    public SignInManager<TUser> SignInManager { get; }
    public UserManager<TUser> UserManager { get; }
    public RoleManager<IdentityRole> RoleManager { get; }
    public ITokenGeneratorService<TUser> TokenGenerator { get; }
    public ITokenValidatorService TokenValidator { get; }
    public IUserValidatorService UserValidator { get; }

    public string GetPhoneNumber(string phoneNumber);
}
