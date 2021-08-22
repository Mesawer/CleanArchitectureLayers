using Mesawer.DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Mesawer.ApplicationLayer.Constants;
using static Mesawer.InfrastructureLayer.Persistence.Database;

namespace Mesawer.InfrastructureLayer.Persistence
{
    public class SessionConfiguration<TSession> : IEntityTypeConfiguration<TSession> where TSession : Session
    {
        public void Configure(EntityTypeBuilder<TSession> entity)
        {
            entity.HasKey(s => s.UserId);

            entity.Property(s => s.UserId).HasMaxLength(IdMaxLength);
            entity.Property(s => s.Token).HasMaxLength(UriMaxLength);
            entity.Property(s => s.MacAddress).HasMaxLength(MacMaxLength);

            entity.Property(s => s.LastLogin).HasColumnType(DateTimeOffset);
        }
    }
}
