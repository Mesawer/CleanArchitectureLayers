using System;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.DomainLayer.CustomIdentity.Entities;
using Mesawer.DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mesawer.ApplicationLayer.CustomIdentity.Interfaces
{
    public interface IIdentityDbContext<TUser, TAccount, TSession, TRole> : IDbContext<TSession>
        where TUser : ApplicationUser
        where TAccount : Account<TUser, TSession, TRole>
        where TSession : Session
        where TRole : Enum
    {
        public DbSet<TAccount> Accounts { get; }
        public DbSet<TUser> Users { get; }
        public DbSet<SocialLogin> SocialLogins { get; }
    }
}
