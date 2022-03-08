using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using Mesawer.ApplicationLayer.Extensions;
using Mesawer.DomainLayer.ValueObjects;

namespace Mesawer.ApplicationLayer.Models
{
    public class PagingOptionsRequest
    {
        public int? Offset { get; set; }

        public int? PageIndex { get; set; }

        public string Search { get; set; }

        public bool Ascending { get; set; }
        public string SortBy { get; set; }

        public class PagingOptionsValidator<T> : AbstractValidator<T> where T : PagingOptionsRequest
        {
            public PagingOptionsValidator()
            {
                RuleFor(c => c.Offset)
                    .InclusiveBetween(0, 99999);

                RuleFor(c => c.PageIndex)
                    .InclusiveBetween(-99999, 99999);
            }
        }

        public IQueryable<T> Handle<T>(IQueryable<T> query, bool all)
        {
            var (asc, page) = GetPageIndex();
            var offset = GetOffset();

            if (!all)
                query = asc
                    ? query.Skip(page * offset).Take(offset)
                    : query.SkipLast(Math.Abs(page) * offset).TakeLast(offset);

            var sortProperty = typeof(T).GetProperties(BindingFlags.Public)
                .FirstOrDefault(p => p.CanRead && p.Name.ToLower() == SortBy);

            if (sortProperty is null) return query;

            var keySelector  = typeof(T).CreateSelectorExpression<T>(sortProperty.Name)?.Body.ToString();

            if (keySelector is null) return query;

            var propertyName = keySelector[(keySelector.IndexOf('.') + 1)..];

            return Ascending ? query.OrderBy(propertyName) : query.OrderByDescending(propertyName);
        }

        public IQueryable<T> ApplyQuery<T>(
            IQueryable<T> queryable,
            IEnumerable<Expression<Func<T, object>>> expressions)
        {
            var list = expressions?.ToList() ?? new List<Expression<Func<T, object>>>();

            if (string.IsNullOrEmpty(Search) || !list.Any()) return queryable;

            var search = Search.Trim().ToLower();

            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), 1, new[] { typeof(string) })!;

            var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), 0, Array.Empty<Type>())!;

            var toStringMethod = typeof(object).GetMethod(nameof(ToString))!;

            var obj = Expression.Parameter(typeof(T), list.First().Parameters.First().Name);

            var comparisonExpressions = list
                .SelectMany(expression =>
                {
                    var copiedExpression = ExpressionConverter<T>.Convert(expression, obj);

                    var bodyType = copiedExpression.Body.Type;

                    if (bodyType == typeof(LocalizedString))
                    {
                        var localExpressions = copiedExpression.BuildExpressions<T, LocalizedString>();

                        return localExpressions.Select(AppendMethods).ToList();
                    }

                    if (bodyType == typeof(FullName))
                    {
                        var localExpressions = copiedExpression.BuildExpressions<T, LocalizedString>();

                        return localExpressions.Select(AppendMethods).ToList();
                    }

                    Expression AppendMethods(Expression ex)
                    {
                        var afterToString = Expression.Call(ex, toStringMethod); // for safety
                        var afterToLower  = Expression.Call(afterToString, toLowerMethod);
                        return Expression.Call(afterToLower, containsMethod, Expression.Constant(search));
                    }

                    return new List<Expression> { AppendMethods(expression.Body) };
                })
                .ToArray();

            var orExpression = ReflectionExtensions.Or(comparisonExpressions);

            var lambdaExpression = Expression.Lambda<Func<T, bool>>(orExpression, obj);

            return queryable
                .Where(lambdaExpression)
                .AsQueryable();
        }

        public PageInfo GetPageInfo(int total)
            => new()
            {
                PageIndex  = PageIndex ?? default,
                TotalPages = (int) Math.Ceiling(total / (double) GetOffset()),
                TotalCount = total,
            };

        private int GetOffset() => Offset is null or <= 0 ? 25 : (int) Offset;

        private (bool asc, int page) GetPageIndex()
        {
            var page = PageIndex ?? default;

            if (page == default) return (true, page);

            return page < 0 ? (false, page + 1) : (true, page);
        }
    }
}
