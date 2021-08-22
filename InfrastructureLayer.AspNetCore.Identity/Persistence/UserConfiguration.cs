using Mesawer.DomainLayer.AspNetCore.Identity.Entities;
using Mesawer.DomainLayer.Entities;
using Mesawer.InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Mesawer.ApplicationLayer.Constants;
using static Mesawer.InfrastructureLayer.Persistence.Database;

namespace Mesawer.InfrastructureLayer.AspNetCore.Identity.Persistence
{
    public class UserConfiguration<TUser, TSession> : DomainDrivenEntityConfiguration<TUser>
        where TUser : ApplicationUser where TSession : Session
    {
        public override void Configure(EntityTypeBuilder<TUser> entity)
        {
            base.Configure(entity);

            entity.Property(u => u.Id).HasMaxLength(IdMaxLength);
            entity.Property(u => u.UserName).HasMaxLength(MaxUserName);
            entity.Property(u => u.NormalizedUserName).HasMaxLength(MaxUserName);
            entity.Property(u => u.PhoneNumber).HasMaxLength(MaxPhoneNumber);

            entity.Property(u => u.CreatedAt).HasColumnType(DateTimeOffset);

            entity.HasOne<TSession>()
                .WithOne()
                .HasForeignKey<TSession>(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
