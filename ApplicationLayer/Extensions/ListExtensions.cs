using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Extensions;

[PublicAPI]
public static class ListExtensions
{
    /// <summary>
    /// Adds a list of items to the passed in source.
    /// </summary>
    public static void AddRange<T>(this IList<T> source, IEnumerable<T> items)
    {
        foreach (var item in items) source.Add(item);
    }

    public static void InsertRange<T>(this IList<T> source, int index, IEnumerable<T> items)
    {
        foreach (var item in items) source.Insert(index++, item);
    }

    public static int FindIndex<T>(this IList<T> source, Predicate<T> selector)
    {
        for (var i = 0; i < source.Count; ++i)
            if (selector(source[i]))
                return i;

        return -1;
    }

    public static void AddFirst<T>(this IList<T> source, T item) => source.Insert(0, item);

    public static void AddLast<T>(this IList<T> source, T item) => source.Insert(source.Count, item);

    public static void InsertAfter<T>(this IList<T> source, T existingItem, T item)
    {
        var index = source.IndexOf(existingItem);
        if (index < 0)
        {
            source.AddFirst(item);
            return;
        }

        source.Insert(index + 1, item);
    }

    public static void InsertAfter<T>(this IList<T> source, Predicate<T> selector, T item)
    {
        var index = source.FindIndex(selector);
        if (index < 0)
        {
            source.AddFirst(item);
            return;
        }

        source.Insert(index + 1, item);
    }

    public static void InsertBefore<T>(this IList<T> source, T existingItem, T item)
    {
        var index = source.IndexOf(existingItem);

        if (index < 0)
        {
            source.AddLast(item);
            return;
        }

        source.Insert(index, item);
    }

    public static void InsertBefore<T>(this IList<T> source, Predicate<T> selector, T item)
    {
        var index = source.FindIndex(selector);

        if (index < 0)
        {
            source.AddLast(item);
            return;
        }

        source.Insert(index, item);
    }

    public static void ReplaceWhile<T>(this IList<T> source, Predicate<T> selector, T item)
    {
        for (var i = 0; i < source.Count; i++)
            if (selector(source[i]))
                source[i] = item;
    }

    public static void ReplaceWhile<T>(this IList<T> source, Predicate<T> selector, Func<T, T> itemFactory)
    {
        for (var i = 0; i < source.Count; i++)
        {
            var item                      = source[i];
            if (selector(item)) source[i] = itemFactory(item);
        }
    }

    public static void ReplaceFirst<T>(this IList<T> source, Predicate<T> selector, T item)
    {
        for (var i = 0; i < source.Count; i++)
            if (selector(source[i]))
            {
                source[i] = item;
                return;
            }
    }

    public static void ReplaceFirst<T>(this IList<T> source, Predicate<T> selector, Func<T, T> itemFactory)
    {
        for (var i = 0; i < source.Count; i++)
        {
            var item = source[i];

            if (!selector(item)) continue;

            source[i] = itemFactory(item);
            return;
        }
    }

    public static void ReplaceFirst<T>(this IList<T> source, T item, T replaceWith)
    {
        for (var i = 0; i < source.Count; i++)
            if (Comparer<T>.Default.Compare(source[i], item) == 0)
            {
                source[i] = replaceWith;
                return;
            }
    }

    public static void MoveItem<T>(this List<T> source, Predicate<T> selector, int targetIndex)
    {
        var len = source.Count - 1;

        if (!targetIndex.ExclusiveBetween(0, len))
            throw new IndexOutOfRangeException($"targetIndex should be between 0 and {len}");

        var currentIndex = source.FindIndex(0, selector);
        if (currentIndex == targetIndex) return;

        var item = source[currentIndex];
        source.RemoveAt(currentIndex);
        source.Insert(targetIndex, item);
    }
}
