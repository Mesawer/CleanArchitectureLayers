using System;
using System.Collections.Generic;
using System.Linq;
using Mesawer.ApplicationLayer.Helpers;

namespace Mesawer.ApplicationLayer.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Projects an object to the respective target by calling the static mapping method and send the optional parameters
    /// </summary>
    public static IEnumerable<TTarget> ProjectTo<TTarget, TSource>(
        this IEnumerable<TSource> enumerable,
        params object[] optionalParameters)
        where TSource : class
    {
        var method = typeof(TTarget).GetProjectToMethod(typeof(TSource), optionalParameters.Length);

        return enumerable.Select(c => method.InvokeProjectToMethod<TTarget>(c, optionalParameters));
    }

    /// <summary>
    /// Returns the distinct elements from a sequence using the passed in projection
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> projection)
        => enumerable.Distinct(new ProjectionEqualityComparer<T, TKey>(projection));

    /// <summary>
    /// Maps an enumerable to key/value tuples
    /// </summary>
    public static IEnumerable<(string Key, T Value)> ToKeyValueTuples<T>(this IEnumerable<T> col) where T : class
    {
        static bool TryGetProp(T obj, string name, out T value)
        {
            var prop  = obj.GetType().GetProperty(name);
            var field = obj.GetType().GetField(name);

            if (prop != null)
            {
                value = (T) prop.GetValue(obj, null);
                return true;
            }

            if (field != null)
            {
                value = (T) field.GetValue(obj);
                return true;
            }

            value = null;
            return false;
        }

        bool IsTuple2(T item, out T name, out T val)
        {
            name = null;
            val  = null;
            return
                item.GetType().Name.OrdinalContains("Tuple") &&
                TryGetProp(item, "Item1", out name) &&
                TryGetProp(item, "Item2", out val) &&
                !TryGetProp(item, "Item3", out _);
        }

        bool LooksLikeKeyValue(T item, out T name, out T val)
        {
            name = null;
            val  = null;
            return
                (TryGetProp(item, "Key", out name) || TryGetProp(item, "key", out name) ||
                 TryGetProp(item, "Name", out name) || TryGetProp(item, "name", out name)) &&
                (TryGetProp(item, "Value", out val) || TryGetProp(item, "value", out val));
        }

        foreach (var item in col)
        {
            if (item == null) continue;
            if (!IsTuple2(item, out var name, out var val) && !LooksLikeKeyValue(item, out name, out val))
                yield return (item.ToInvariantString(), null);
            else if (name != null) yield return (name.ToInvariantString(), val);
        }
    }

    /// <summary>
    /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var obj in source) action(obj);
    }

    /// <summary>
    /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static void ForEach<T>(
        this IEnumerable<T> source,
        Action<int, T> action)
    {
        var index = 0;
        foreach (var obj in source) action(index++, obj);
    }

    /// <summary>
    /// Concatenates the members of a collection, using the specified separator between each member.
    /// This is a shortcut for string.Join(...)
    /// </summary>
    /// <param name="source">A collection that contains the objects to concatenate.</param>
    /// <param name="separator">
    /// The string to use as a separator. separator is included in the returned string only if values
    /// has more than one element.
    /// </param>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <returns>
    /// A string that consists of the members of values delimited by the separator string. If values has no
    /// members,
    /// the method returns System.String.Empty.
    /// </returns>
    public static string JoinAsString<T>(this IEnumerable<T> source, string separator)
        => string.Join(separator, source);

    /// <summary>
    /// Filters a <see cref="IEnumerable{T}"/> by given predicate if given condition is true.
    /// </summary>
    /// <param name="source">Enumerable to apply filtering</param>
    /// <param name="condition">A boolean value</param>
    /// <param name="predicate">Predicate to filter the enumerable</param>
    /// <returns>Filtered or not filtered enumerable based on <paramref name="condition"/></returns>
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
        => condition ? source.Where(predicate) : source;

    /// <summary>
    /// Filters a <see cref="IEnumerable{T}"/> by given predicate if given condition is true.
    /// </summary>
    /// <param name="source">Enumerable to apply filtering</param>
    /// <param name="condition">A boolean value</param>
    /// <param name="predicate">Predicate to filter the enumerable</param>
    /// <returns>Filtered or not filtered enumerable based on <paramref name="condition"/></returns>
    public static IEnumerable<T> WhereIf<T>(
        this IEnumerable<T> source,
        bool condition,
        Func<T, int, bool> predicate)
        => condition ? source.Where(predicate) : source;

    /// <summary>
    /// Checks if the passed enumerable has duplicates.
    /// </summary>
    public static bool HasDuplicates<T, TProp>(this IEnumerable<T> enumerable, Func<T, TProp> selector)
    {
        var d = new HashSet<TProp>();

        return enumerable.Any(t => !d.Add(selector(t)));
    }

    /// <summary>
    /// Returns the duplicates of the passed enumerable.
    /// </summary>
    public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> enumerable)
        => enumerable
            .GroupBy(c => c)
            .Where(g => g.Count() > 1)
            .Select(y => y.Key);
}
