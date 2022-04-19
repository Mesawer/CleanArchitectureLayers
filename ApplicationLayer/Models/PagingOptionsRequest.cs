using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            var keySelector = typeof(T).CreateSelectorExpression<T>(SortBy)?.Body.ToString();

            if (keySelector is not null)
            {
                var propertyName = keySelector[(keySelector.IndexOf('.') + 1)..];

                query = Ascending
                    ? query.OrderBy(propertyName).AsQueryable()
                    : query.OrderByDescending(propertyName).AsQueryable();
            }

            var (asc, page) = GetPageIndex();
            var offset = GetOffset();

            if (all) return query;

            return asc
                ? query.Skip(page * offset).Take(offset)
                : query.SkipLast(Math.Abs(page) * offset).TakeLast(offset);
        }

        public IQueryable<T> ApplyQuery<T>(
            IQueryable<T> queryable,
            IEnumerable<Expression<Func<T, object>>> expressions)
        {
            var list = expressions?.ToList() ?? new List<Expression<Func<T, object>>>();

            if (string.IsNullOrEmpty(Search) || !list.Any()) return queryable;

            var search = Search.Trim().ToLower();

            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), 0, new[] { typeof(string) })!;

            var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), 0, Array.Empty<Type>())!;

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
                        var localExpressions = copiedExpression.BuildExpressions<T, FullName>();

                        return localExpressions.Select(AppendMethods).ToList();
                    }

                    Expression AppendMethods(Expression ex)
                    {
                        var afterToLower = Expression.Call(ex, toLowerMethod);

                        return Expression.Call(afterToLower, containsMethod, Expression.Constant(search));
                    }

                    return new List<Expression> { AppendMethods(copiedExpression.Body) };
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
