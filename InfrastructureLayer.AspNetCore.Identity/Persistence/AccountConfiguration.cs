using Mesawer.DomainLayer.AspNetCore.Identity.Entities;
using Mesawer.InfrastructureLayer.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Mesawer.ApplicationLayer.Constants;

namespace Mesawer.InfrastructureLayer.AspNetCore.Identity.Persistence
{
    public class AccountConfiguration<TUser, TAccount> : IEntityTypeConfiguration<TAccount>
        where TUser : ApplicationUser where TAccount : Account<TUser>
    {
        public virtual void Configure(EntityTypeBuilder<TAccount> entity)
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Id).HasMaxLength(IdMaxLength);

            entity.OwnsFullName(a => a.Name);

            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<IdentityRole>()
                .WithMany()
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
