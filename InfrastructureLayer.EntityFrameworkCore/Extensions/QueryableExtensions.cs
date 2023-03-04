using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Mesawer.ApplicationLayer.Extensions;
using Mesawer.ApplicationLayer.Models;
using Mesawer.DomainLayer.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Mesawer.InfrastructureLayer.EntityFrameworkCore.Extensions;

[PublicAPI]
public static class QueryableExtensions
{
    /// <summary>
    /// Creates a grouping list from the passed in queryable by using the passed in key selector.
    /// </summary>
    public static async Task<List<Grouping<TTarget>>> ToGroupingAsync<TTarget, TSource, TKey>(
        this IQueryable<TSource> queryable,
        Func<TSource, TKey> keySelector,
        CancellationToken ct,
        params object[] optionalParameters)
        where TSource : class
        => (await queryable.ToListAsync(ct))
            .GroupBy(keySelector)
            .Select(c => new Grouping<TTarget>
            {
                Key   = Convert.ToInt32(c.Key),
                Items = c.ProjectTo<TTarget, TSource>(optionalParameters).ToList()
            })
            .ToList();

    public static IQueryable<T> LocalizedStringContains<T>(this IQueryable<T> query, string value)
    {
        var type = typeof(LocalizedString);

        var localizedString = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == type)
            .ToList();

        if (!localizedString.Any()) return query;

        var obj = Expression.Parameter(typeof(T));

        var contains = typeof(string).GetMethods()
            .First(m => m.Name == nameof(string.Contains) && m.GetParameters().Length == 1 &&
                        m.GetParameters().First().ParameterType == typeof(string));

        var toLower = typeof(string).GetMethods()
            .First(m => m.Name == nameof(string.ToLower) && !m.GetParameters().Any());

        // Build up the LINQ expression backwards:
        // query = query.Where(x => x.Property.Ar.ToLower().Contains(value) || x.Property.En.ToLower().Contains(value));

        // x.Property
        var properties = localizedString.Select(p => Expression.Property(obj, p));

        var comparisonExpressions = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            // x.Property.Ar
            .Select(p => properties.Select(c => Expression.Property(c, p)))
            .SelectMany(p => p)
            // x.Property.Ar.ToLower().Contains(value)
            .Select(p => (Expression) Expression.Call(Expression.Call(p, toLower),
                contains,
                Expression.Constant(value)));

        // x.Property.Ar.ToLower().Contains(value) || x.Property.En.ToLower().Contains(value)
        var orExpression = ReflectionExtensions.Or(comparisonExpressions.ToArray());

        // x => x.Property.Ar.ToLower().Contains(value) || x.Property.En.ToLower().Contains(value)
        var lambdaExpression = Expression.Lambda<Func<T, bool>>(orExpression, obj);

        // query = query.Where...
        return query.Where(lambdaExpression).AsQueryable();
    }

    public static Task<bool> LocalizedStringAllNotEquals<T>(
        this IQueryable<T> query,
        PropertyInfo property,
        string lang,
        string value,
        CancellationToken ct)
    {
        var langProperty = typeof(LocalizedString)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(p => p.Name == lang);

        if (langProperty is null) return Task.FromResult(true);

        var toLower = typeof(string).GetMethods()
            .First(m => m.Name == nameof(string.ToLower) && !m.GetParameters().Any());

        var obj = Expression.Parameter(typeof(T));

        // x.property.langProperty
        var exp1       = Expression.Property(obj, property);
        var expression = Expression.Property(exp1, langProperty);

        // x.property.langProperty.ToLower() != value.Trim().ToLower()
        var comparisonExpressions =
            (Expression) Expression.NotEqual(Expression.Call(expression, toLower),
                Expression.Constant(value.Trim().ToLower()));

        var lambdaExpression = Expression.Lambda<Func<T, bool>>(comparisonExpressions, obj);

        // query = query.Where...
        return query.AllAsync(lambdaExpression, ct);
    }

    public static async Task<bool> LocalizedStringOnlyNotEquals<T, TKey>(
        this IQueryable<T> query,
        PropertyInfo keyProperty,
        PropertyInfo property,
        string lang,
        TKey key,
        string value,
        CancellationToken ct)
    {
        var langProperty = typeof(LocalizedString)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(p => p.Name == lang);

        if (langProperty is null) return true;

        var obj = Expression.Parameter(typeof(T));

        // x.property.langProperty
        var exp1       = Expression.Property(obj, property);
        var expression = Expression.Property(exp1, langProperty);

        // x.property.langProperty == value.Trim().ToLower()
        var comparisonExpressions =
            (Expression) Expression.Equal(expression, Expression.Constant(value.Trim().ToLower()));

        var lambdaExpression = Expression.Lambda<Func<T, bool>>(comparisonExpressions, obj);

        var matches = await query.Where(lambdaExpression).ToListAsync(ct);

        if (!matches.Any()) return true;
        if (matches.Count > 1) return false;

        return keyProperty.GetValue(matches.First())?.Equals(key) ?? true;
    }

    /// <summary>
    /// Checks if a property's value exits in the passed in values.
    /// </summary>
    public static Task<bool> In<T, TKey>(
        this IQueryable<T> queryable,
        Expression<Func<T, TKey>> expression,
        params TKey[] values)
    {
        if ( values is null || !values.Any()) return Task.FromResult(false);

        // key == value1, key == value2, ...
        var parts = values
            .Select(v => (Expression) Expression.Equal(expression.Body, Expression.Constant(v)))
            .ToArray();

        // key == value1 || key == value2 || ...
        var comparisonExpression = ReflectionExtensions.Or(parts);

        // x => key == value1 || key == value2 || ...
        var lambdaExpression = Expression.Lambda<Func<T, bool>>(comparisonExpression, expression.Parameters);

        return queryable.AnyAsync(lambdaExpression);
    }
}
