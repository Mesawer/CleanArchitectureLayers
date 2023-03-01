using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Models;

[PublicAPI]
public class ExpressionAggregator : ExpressionVisitor
{
    private Expression _left;
    private Expression _right;

    public Expression<TDelegate> AddExpression<TDelegate>(
        Expression<TDelegate> left,
        Expression<TDelegate> right)
    {
        if (left?.Body is null || right?.Body is null) throw new ArgumentNullException();

        _left  = left.Parameters[0];
        _right = right.Parameters[0];

        return Expression.Lambda<TDelegate>(
            Expression.AndAlso(Visit(left.Body)!, right.Body),
            right.Parameters);
    }

    public Expression<TDelegate> OrExpression<TDelegate>(
        Expression<TDelegate> left,
        Expression<TDelegate> right)
    {
        if (left?.Body is null || right?.Body is null) throw new ArgumentNullException();

        _left  = left.Parameters[0];
        _right = right.Parameters[0];

        return Expression.Lambda<TDelegate>(
            Expression.OrElse(Visit(left.Body)!, right.Body),
            right.Parameters);
    }

    public override Expression Visit(Expression node) => _left == node ? _right : base.Visit(node);
}
