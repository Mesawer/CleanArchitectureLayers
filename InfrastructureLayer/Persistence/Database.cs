using System;
using System.Linq.Expressions;
using Mesawer.DomainLayer.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Mesawer.ApplicationLayer.Constants;

namespace Mesawer.InfrastructureLayer.Persistence
{
    public static class Database
    {
        public const string DateTimeOffset = "datetimeoffset";

        public static void OwnsFullName<TEntity>(
            this EntityTypeBuilder<TEntity> entity,
            Expression<Func<TEntity, FullName>> navigationExpression) where TEntity : class
            => entity.OwnsOne(navigationExpression, FullNameTypeBuilder);

        public static void OwnsLocalizedString<TEntity>(
            this EntityTypeBuilder<TEntity> entity,
            Expression<Func<TEntity, LocalizedString>> navigationExpression,
            int maxLength = NameMaxLength) where TEntity : class
            => entity.OwnsOne(navigationExpression, c => LocalizedNameTypeBuilder(c, maxLength));

        private static void FullNameTypeBuilder<TEntity>(OwnedNavigationBuilder<TEntity, FullName> builder)
            where TEntity : class
        {
            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(NameMaxLength);

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(NameMaxLength);
        }

        private static void LocalizedNameTypeBuilder<TEntity>(
            OwnedNavigationBuilder<TEntity, LocalizedString> builder,
            int maxLength)
            where TEntity : class
        {
            builder.Property(e => e.Ar)
                .IsRequired()
                .HasMaxLength(maxLength);

            builder.Property(e => e.En)
                .IsRequired()
                .HasMaxLength(maxLength);
        }
    }
}
