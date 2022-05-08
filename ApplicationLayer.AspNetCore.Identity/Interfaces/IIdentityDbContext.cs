using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.DomainLayer.AspNetCore.Identity.Entities;
using Mesawer.DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces;

public interface IIdentityDbContext<TUser, TAccount, TSession> : IDbContext<TSession>
    where TUser : ApplicationUser where TAccount : Account<TUser> where TSession : Session
{
    public DbSet<TUser> Users { get; }
    public DbSet<TAccount> Accounts { get; }
}
