using System;
using System.Collections.Generic;

namespace Mesawer.ApplicationLayer.Helpers;

public class ProjectionEqualityComparer<TSource, TKey>
    : IEqualityComparer<TSource>
{
    private readonly Func<TSource, TKey>     _projection;
    private readonly IEqualityComparer<TKey> _comparer;

    public ProjectionEqualityComparer(
        Func<TSource, TKey> projection,
        IEqualityComparer<TKey> comparer = null)
    {
        _comparer   = comparer ?? EqualityComparer<TKey>.Default;
        _projection = projection ?? throw new ArgumentNullException(nameof(projection));
    }

    public bool Equals(TSource x, TSource y)
    {
        if (x is null && y is null) return true;

        if (x is null || y is null) return false;

        return _comparer.Equals(_projection(x), _projection(y));
    }

    public int GetHashCode(TSource obj)
    {
        if (obj is null) throw new ArgumentNullException(nameof(obj));

        return _comparer.GetHashCode(_projection(obj));
    }
}
