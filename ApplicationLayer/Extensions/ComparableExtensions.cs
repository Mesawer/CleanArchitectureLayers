using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Extensions;

[PublicAPI]
public static class ComparableExtensions
{
    /// <summary>
    /// Checks whether the value is in a range between the two specified
    /// numbers (exclusive).
    /// </summary>
    /// <param name="value">The value to be checked</param>
    /// <param name="minValue">Minimum (exclusive) value</param>
    /// <param name="maxValue">Maximum (exclusive) value</param>
    public static bool ExclusiveBetween<T>(this T value, T minValue, T maxValue)
        where T : IComparable<T>
        => value.CompareTo(minValue) > 0 && value.CompareTo(maxValue) < 0;

    /// <summary>
    /// Checks whether the value is in a range between the two specified
    /// numbers (inclusive).
    /// </summary>
    /// <param name="value">The value to be checked</param>
    /// <param name="minValue">Minimum (inclusive) value</param>
    /// <param name="maxValue">Maximum (inclusive) value</param>
    public static bool InclusiveBetween<T>(this T value, T minValue, T maxValue)
        where T : IComparable<T>
        => value.CompareTo(minValue) >= 0 && value.CompareTo(maxValue) <= 0;

    /// <summary>
    /// Checks whether a certain enumerable contains any of the passed values.
    /// </summary>
    public static bool HasAny<T>(this IEnumerable<T> source, params object[] values)
        where T : IComparable
        => source.Select(item => values.Any(c => item.CompareTo(c) == 0)).FirstOrDefault();

    /// <summary>
    /// Checks whether a certain enumerable contains any of the passed values.
    /// </summary>
    public static bool HasAny<T>(this IEnumerable<T> source, params T[] values)
        where T : IComparable<T>
        => source.Select(item => values.Any(c => item.CompareTo(c) == 0)).FirstOrDefault();

    /// <summary>
    /// Checks whether a certain enumerable contains all of the passed values.
    /// </summary>
    public static bool HasAll<T>(this IEnumerable<T> source, params object[] values)
        where T : IComparable
        => values.All(item => source.Any(c => c.CompareTo(item) == 0));

    /// <summary>
    /// Checks whether a certain enumerable contains all of the passed values.
    /// </summary>
    public static bool HasAll<T>(this IEnumerable<T> source, params T[] values)
        where T : IComparable<T>
        => values.All(item => source.Any(c => c.CompareTo(item) == 0));
}
