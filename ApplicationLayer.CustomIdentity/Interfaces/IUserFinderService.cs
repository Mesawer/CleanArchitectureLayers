using System.Threading;
using System.Threading.Tasks;
using Mesawer.DomainLayer.CustomIdentity.Entities;

namespace Mesawer.ApplicationLayer.CustomIdentity.Interfaces
{
    public interface IUserFinderService<TUser> where TUser : ApplicationUser
    {
        Task<TUser> FindByIdAsync(string id, CancellationToken ct = default);
        Task<TUser> FindByEmailAsync(string email, CancellationToken ct = default);
        Task<TUser> FindByNameAsync(string username, CancellationToken ct = default);
    }
}
