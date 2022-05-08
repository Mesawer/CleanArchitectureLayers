using System;
using System.Linq;
using System.Linq.Expressions;

namespace Mesawer.ApplicationLayer.Models;

public class ExpressionConverter<TTarget>
{
    public class ConversionVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _newParameter;
        private readonly ParameterExpression _oldParameter;

        public ConversionVisitor(ParameterExpression newParameter, ParameterExpression oldParameter)
        {
            _newParameter = newParameter;
            _oldParameter = oldParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => _newParameter; // replace all old param references with new ones

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != _oldParameter) // if instance is not old parameter - do nothing
                return base.VisitMember(node);

            var newObj    = Visit(node.Expression);
            var newMember = _newParameter.Type.GetMember(node.Member.Name).First();

            return Expression.MakeMemberAccess(newObj, newMember);
        }
    }

    public static Expression<Func<TTarget, TR>> Convert<TSource, TR>(
        Expression<Func<TSource, TR>> e,
        ParameterExpression newParameter)
    {
        var oldParameter = e.Parameters[0];

        var converter = new ConversionVisitor(newParameter, oldParameter);
        var newBody   = converter.Visit(e.Body);

        return Expression.Lambda<Func<TTarget, TR>>(newBody, newParameter);
    }
}
