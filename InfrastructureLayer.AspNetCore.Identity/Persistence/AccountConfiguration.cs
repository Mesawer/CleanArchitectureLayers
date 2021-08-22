using Mesawer.DomainLayer.AspNetCore.Identity.Entities;
using Mesawer.DomainLayer.Entities;
using Mesawer.InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mesawer.InfrastructureLayer.AspNetCore.Identity.Persistence
{
    public class AccountConfiguration<TUser, TAccount, TSession> : IEntityTypeConfiguration<TAccount>
        where TUser : ApplicationUser where TAccount : Account<TUser, TSession> where TSession : Session
    {
        public virtual void Configure(EntityTypeBuilder<TAccount> entity)
        {
            entity.OwnsFullName(a => a.Name);

            entity.HasOne(a => a.Session)
                .WithOne()
                .HasForeignKey<TSession>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
