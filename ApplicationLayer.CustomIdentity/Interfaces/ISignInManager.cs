using System.Threading.Tasks;
using Mesawer.ApplicationLayer.CustomIdentity.Models;
using Mesawer.DomainLayer.CustomIdentity.Entities;

namespace Mesawer.ApplicationLayer.CustomIdentity.Interfaces
{
    public interface ISignInManager<in TUser> where TUser : ApplicationUser
    {
        Task<IdentityResult> SignInAsync(TUser user, string password);
        Task SignOutAsync();
    }
}
