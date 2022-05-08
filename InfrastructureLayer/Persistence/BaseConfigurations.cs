using Mesawer.DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Mesawer.ApplicationLayer.Constants;
using static Mesawer.InfrastructureLayer.Persistence.Database;

namespace Mesawer.InfrastructureLayer.Persistence;

public class DomainDrivenEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : class, IHasDomainEvent
{
    public virtual void Configure(EntityTypeBuilder<T> entity) => entity.Ignore(e => e.DomainEvents);
}

public class AuditableEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : AuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<T> entity)
    {
        entity.Property(e => e.Created).HasColumnType(DateTimeOffset);
        entity.Property(e => e.CreatedBy).HasMaxLength(IdMaxLength);

        entity.Property(e => e.LastModified).HasColumnType(DateTimeOffset);
        entity.Property(e => e.LastModifiedBy).HasMaxLength(IdMaxLength);
    }
}

public class DomainDrivenAuditableEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : AuditableEntity, IHasDomainEvent
{
    public virtual void Configure(EntityTypeBuilder<T> entity)
    {
        entity.Ignore(e => e.DomainEvents);

        entity.Property(e => e.Created).HasColumnType(DateTimeOffset);
        entity.Property(e => e.CreatedBy).HasMaxLength(IdMaxLength);

        entity.Property(e => e.LastModified).HasColumnType(DateTimeOffset);
        entity.Property(e => e.LastModifiedBy).HasMaxLength(IdMaxLength);
    }
}
