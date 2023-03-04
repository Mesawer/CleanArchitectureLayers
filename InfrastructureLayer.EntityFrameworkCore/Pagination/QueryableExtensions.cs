using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Mesawer.ApplicationLayer.Models;

namespace Mesawer.InfrastructureLayer.EntityFrameworkCore.Pagination;

[PublicAPI]
public static class QueryableExtensions
{
    /// <summary>
    /// Creates a paginated list from the passed in queryable by applying the paging options request.
    /// </summary>
    public static Task<PaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable,
        PagingOptionsRequest options,
        CancellationToken ct = default)
        => PaginatedListFactory<TDestination>.CreateAsync(queryable, options, false, ct);

    /// <summary>
    /// Creates a paginated list from the passed in queryable by applying the paging options request.
    /// </summary>
    /// <remarks>It returns all the items in the first page.</remarks>
    public static Task<PaginatedList<TDestination>> ToAllPaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable,
        PagingOptionsRequest options,
        CancellationToken ct = default)
        => PaginatedListFactory<TDestination>.CreateAsync(queryable, options, true, ct);

    /// <summary>
    /// Applies the search options using the passed in expressions on the passed in queryable.
    /// </summary>
    public static IQueryable<T> Search<T>(
        this IQueryable<T> queryable,
        PagingOptionsRequest options,
        params Expression<Func<T, object>>[] expressions)
        => options.ApplyQuery(queryable, expressions);
}
