using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using Mesawer.ApplicationLayer.Extensions;

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

            var keySelector = typeof(T).CreateSelectorExpression<T>(sortProperty.Name);

            return Ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }

        public IQueryable<T> ApplyQuery<T>(
            IQueryable<T> queryable,
            IEnumerable<Expression<Func<T, object>>> expressions)
        {
            var list = expressions?.ToList() ?? new List<Expression<Func<T, object>>>();

            if (string.IsNullOrEmpty(Search) || !list.Any()) return queryable;

            var search = Search.Trim().ToLower();

            var containsMethod = typeof(string).GetMethods().First(m
                => m.Name == nameof(string.Contains) &&
                   m.GetParameters().Length == 1 &&
                   m.GetParameters().First().ParameterType == typeof(string));

            var toLowerMethod = typeof(string).GetMethods()
                .First(m => m.Name == nameof(string.ToLower) && !m.GetParameters().Any());

            var obj = Expression.Parameter(typeof(T), list.First().Parameters.First().Name);

            var comparisonExpressions = list
                .Select(expression =>
                {
                    var copiedExpression = ExpressionConverter<T>.Convert(expression, obj);

                    return (Expression) Expression.Call(Expression.Call(copiedExpression.Body, toLowerMethod),
                        containsMethod,
                        Expression.Constant(search));
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
