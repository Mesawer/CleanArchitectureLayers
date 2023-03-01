using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Mesawer.DomainLayer.ValueObjects;

namespace Mesawer.ApplicationLayer.Extensions;

[PublicAPI]
public static class QueryableExtensions
{
    /// <summary>
    /// Projects an object to the respective target by calling the static mapping method and send the optional parameters
    /// </summary>
    public static IQueryable<TTarget> ProjectTo<TTarget, TSource>(
        this IQueryable<TSource> queryable,
        params object[] optionalParameters)
        where TSource : class
    {
        var method = typeof(TTarget).GetProjectToMethod(typeof(TSource), optionalParameters.Length);

        return queryable.Select(c => method.InvokeProjectToMethod<TTarget>(c, optionalParameters)!);
    }

    /// <summary>
    /// Sorts the elements of a sequence in ascending order.
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="propertyName"/> not valid property name.</exception>
    /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">Source</param>
    /// <param name="propertyName">The property name to order by. You can use '.' to access a child property.</param>
    /// <param name="comparer">An IComparer&lt;object&gt; to compare keys.</param>
    public static IOrderedQueryable<T> OrderBy<T>(
        this IQueryable<T> source,
        string propertyName,
        IComparer<object> comparer = null)
        => CallOrderedQueryable(source, nameof(Queryable.OrderBy), propertyName, comparer);

    /// <summary>
    /// Sorts the elements of a sequence in descending order.
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="propertyName"/> not valid property name.</exception>
    /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">Source</param>
    /// <param name="propertyName">The property name to order by. You can use '.' to access a child property.</param>
    /// <param name="comparer">An IComparer&lt;object&gt; to compare keys.</param>
    public static IOrderedQueryable<T> OrderByDescending<T>(
        this IQueryable<T> source,
        string propertyName,
        IComparer<object> comparer = null)
        => CallOrderedQueryable(source, nameof(Queryable.OrderByDescending), propertyName, comparer);

    /// <summary>
    /// Performs a subsequent ordering of the elements in a sequence in ascending order.
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="propertyName"/> not valid property name.</exception>
    /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">Source</param>
    /// <param name="propertyName">The property name to order by. You can use '.' to access a child property.</param>
    /// <param name="comparer">An IComparer&lt;object&gt; to compare keys.</param>
    public static IOrderedQueryable<T> ThenBy<T>(
        this IQueryable<T> source,
        string propertyName,
        IComparer<object> comparer = null)
        => CallOrderedQueryable(source, nameof(Queryable.ThenBy), propertyName, comparer);

    /// <summary>
    /// Performs a subsequent ordering of the elements in a sequence in descending order.
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="propertyName"/> not valid property name.</exception>
    /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">Source</param>
    /// <param name="propertyName">The property name to order by. You can use '.' to access a child property.</param>
    /// <param name="comparer">An IComparer&lt;object&gt; to compare keys.</param>
    public static IOrderedQueryable<T> ThenByDescending<T>(
        this IQueryable<T> source,
        string propertyName,
        IComparer<object> comparer = null)
        => CallOrderedQueryable(source, nameof(Queryable.ThenByDescending), propertyName, comparer);

    /// <summary>
    /// Builds the Queryable functions using a TSource property name.
    /// </summary>
    private static IOrderedQueryable<T> CallOrderedQueryable<T>(
        this IQueryable<T> source,
        string methodName,
        string propertyName,
        IComparer<object> comparer = null)
    {
        var param = Expression.Parameter(typeof(T), "x");

        Expression body;

        try
        {
            body = propertyName.Split('.')
                .Aggregate<string, Expression>(param, Expression.PropertyOrField);
        }
        catch (Exception)
        {
            throw new ArgumentException($"'{propertyName}' is invalid sorting property.");
        }

        return comparer != null
            ? (IOrderedQueryable<T>) source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new[] { typeof(T), body.Type },
                    source.Expression,
                    Expression.Lambda(body, param),
                    Expression.Constant(comparer)
                )
            )
            : (IOrderedQueryable<T>) source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new[] { typeof(T), body.Type },
                    source.Expression,
                    Expression.Lambda(body, param)
                )
            );
    }

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
}
