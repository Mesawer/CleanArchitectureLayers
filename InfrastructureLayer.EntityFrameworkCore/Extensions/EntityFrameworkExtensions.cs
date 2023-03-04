using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using JetBrains.Annotations;
using Mesawer.ApplicationLayer.Models;
using Mesawer.ApplicationLayer.Resources.Common;
using Mesawer.DomainLayer.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Mesawer.InfrastructureLayer.EntityFrameworkCore.Extensions;

[PublicAPI]
public static class EntityFrameworkExtensions
{
    /// <summary>
    ///     Finds an entity with the given primary key value. If an entity with the given primary key value
    ///     is being tracked by the context, then it is returned immediately without making a request to the
    ///     database. Otherwise, a query is made to the database for an entity with the given primary key value
    ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
    ///     null is returned.
    /// </summary>
    public static Task<T> FindByKeyAsync<T>(this DbSet<T> source, object key, CancellationToken ct = default)
        where T : class
        => source.FindAsync(new[] { key }, ct).AsTask();

    // TODO: Accept predicates
    public static IRuleBuilder<T, LocalizedStringDto> UniqueLocalizedString<T, TEntity>(
        this IRuleBuilder<T, LocalizedStringDto> builder,
        IQueryable<TEntity> queryable,
        string propertyName = null)
        => builder
            .ChildRules(str =>
            {
                var validateAr = LocalizedString.IsValidAr;
                var validateEn = LocalizedString.IsValidEn;

                var property = propertyName is null
                    ? typeof(TEntity)
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .FirstOrDefault(p => p.PropertyType == typeof(LocalizedString))
                    : typeof(TEntity)
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .FirstOrDefault(p => p.Name == propertyName);

                if (property is null) return;

                str.RuleFor(s => s)
                    .MustAsync((s, ct)
                        => queryable.LocalizedStringAllNotEquals(
                            property,
                            nameof(LocalizedStringDto.Ar),
                            s.Ar,
                            ct))
                    .When(s => s is not null && validateAr(s.Ar))
                    .WithName($"{nameof(LocalizedStringDto.Ar)}")
                    .WithMessage(SharedRes.NameAlreadyExists);

                str.RuleFor(s => s)
                    .MustAsync((s, ct)
                        => queryable.LocalizedStringAllNotEquals(
                            property,
                            nameof(LocalizedStringDto.En),
                            s.En,
                            ct))
                    .When(s => s is not null && validateEn(s.En))
                    .WithName($"{nameof(LocalizedStringDto.En)}")
                    .WithMessage(SharedRes.NameAlreadyExists);
            });

    // TODO: Accept predicates
    public static IRuleBuilder<T, Localizable<TKey>> UniqueLocalizedString<T, TEntity, TKey>(
        this IRuleBuilder<T, Localizable<TKey>> builder,
        IQueryable<TEntity> queryable,
        string keyPropertyName,
        string propertyName = null)
        => builder
            .ChildRules(str =>
            {
                var validateAr = LocalizedString.IsValidAr;
                var validateEn = LocalizedString.IsValidEn;

                var keyProperty = keyPropertyName is not null
                    ? typeof(TEntity)
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .FirstOrDefault(p => p.Name == keyPropertyName)
                    : null;

                var property = propertyName is null
                    ? typeof(TEntity)
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .FirstOrDefault(p => p.PropertyType == typeof(LocalizedString))
                    : typeof(TEntity)
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .FirstOrDefault(p => p.Name == propertyName);

                if (property is null) return;

                if (keyProperty is null) return;

                str.RuleFor(s => s)
                    .MustAsync((s, ct)
                        => queryable.LocalizedStringOnlyNotEquals(keyProperty,
                            property,
                            nameof(LocalizedStringDto.Ar),
                            s.Key,
                            s.String?.Ar,
                            ct))
                    .When(s => s.String is not null && validateAr(s.String.Ar))
                    .WithName($"{property.Name}.{nameof(LocalizedStringDto.Ar)}")
                    .WithMessage(SharedRes.NameAlreadyExists);

                str.RuleFor(s => s)
                    .MustAsync((s, ct)
                        => queryable.LocalizedStringOnlyNotEquals(keyProperty,
                            property,
                            nameof(LocalizedStringDto.En),
                            s.Key,
                            s.String?.En,
                            ct))
                    .When(s => s.String is not null && validateEn(s.String.En))
                    .WithName($"{property.Name}.{nameof(LocalizedStringDto.En)}")
                    .WithMessage(SharedRes.NameAlreadyExists);
            });
}
