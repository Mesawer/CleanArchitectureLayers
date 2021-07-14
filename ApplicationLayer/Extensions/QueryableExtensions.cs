using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Models;
using Mesawer.DomainLayer.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Mesawer.ApplicationLayer.Extensions
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Creates a paginated list from the passed in queryable by applying the paging options request.
        /// </summary>
        public static Task<PaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(
            this IQueryable<TDestination> queryable,
            PagingOptionsRequest options,
            CancellationToken ct = default)
            => PaginatedList<TDestination>.CreateAsync(queryable, options, false, ct);

        /// <summary>
        /// Creates a paginated list from the passed in queryable by applying the paging options request.
        /// </summary>
        /// <remarks>It returns all the items in the first page.</remarks>
        public static Task<PaginatedList<TDestination>> ToAllPaginatedListAsync<TDestination>(
            this IQueryable<TDestination> queryable,
            PagingOptionsRequest options,
            CancellationToken ct = default)
            => PaginatedList<TDestination>.CreateAsync(queryable, options, true, ct);

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

        /// <summary>
        /// Applies the search options using the passed in expressions on the passed in queryable.
        /// </summary>
        public static IQueryable<T> Search<T>(
            this IQueryable<T> queryable,
            PagingOptionsRequest options,
            params Expression<Func<T, object>>[] expressions)
            => options.ApplyQuery(queryable, expressions);

        /// <summary>
        /// Projects an object to the respective target by calling the static mapping method and send the optional parameters
        /// </summary>
        public static IQueryable<TTarget> ProjectTo<TTarget, TSource>(
            this IQueryable<TSource> queryable,
            params object[] optionalParameters)
            where TSource : class
        {
            var method = typeof(TTarget).GetProjectToMethod(typeof(TSource), optionalParameters.Length);

            return queryable.Select(c => method.InvokeProjectToMethod<TTarget>(c, optionalParameters)).AsQueryable();
        }

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
            if (values is null || !values.Any()) return Task.FromResult(false);

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
}
