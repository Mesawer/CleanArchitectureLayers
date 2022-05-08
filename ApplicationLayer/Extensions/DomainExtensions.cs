using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mesawer.ApplicationLayer.Extensions;

public static class DomainExtensions
{
    public static IEnumerable<MemberExpression> BuildExpressions<TEntity, TTarget>(
        this Expression<Func<TEntity, object>> expression)
    {
        var bodyType = expression.Body.Type;

        if (!typeof(TTarget).IsOfType(bodyType)) return Enumerable.Empty<MemberExpression>();

        return typeof(TTarget)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => Expression.Property(expression.Body, p))
            .ToArray();
    }
}
