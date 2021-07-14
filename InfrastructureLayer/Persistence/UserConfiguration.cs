using Mesawer.ApplicationLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Mesawer.ApplicationLayer.Constants;
using static Mesawer.InfrastructureLayer.Persistence.Database;

namespace Mesawer.InfrastructureLayer.Persistence
{
    public class UserConfiguration<TUser> : DomainDrivenEntityConfiguration<TUser> where TUser : ApplicationUser
    {
        public override void Configure(EntityTypeBuilder<TUser> entity)
        {
            base.Configure(entity);

            entity.Property(u => u.Id).HasMaxLength(IdMaxLength);
            entity.Property(u => u.UserName).HasMaxLength(MaxUserName);
            entity.Property(u => u.NormalizedUserName).HasMaxLength(MaxUserName);
            entity.Property(u => u.PhoneNumber).HasMaxLength(MaxPhoneNumber);

            entity.Property(u => u.CreatedAt).HasColumnType(DateTimeOffset);
        }
    }
}
