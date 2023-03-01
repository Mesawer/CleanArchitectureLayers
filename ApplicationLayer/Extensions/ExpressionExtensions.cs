using System.Linq.Expressions;
using JetBrains.Annotations;
using Mesawer.ApplicationLayer.Models;

namespace Mesawer.ApplicationLayer.Extensions;

[PublicAPI]
public static class ExpressionExtensions
{
    public static Expression<TDelegate> Add<TDelegate>(this Expression<TDelegate> left, Expression<TDelegate> right)
    {
        var visitor = new ExpressionAggregator();

        return visitor.AddExpression(left, right);
    }

    public static Expression<TDelegate> Or<TDelegate>(this Expression<TDelegate> left, Expression<TDelegate> right)
    {
        var visitor = new ExpressionAggregator();

        return visitor.OrExpression(left, right);
    }
}
