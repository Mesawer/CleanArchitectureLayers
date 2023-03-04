using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Mesawer.ApplicationLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace Mesawer.InfrastructureLayer.EntityFrameworkCore.Pagination;

[PublicAPI]
public class PaginatedListFactory<T>
{
    public static async Task<PaginatedList<TDto>> CreateAsync<TDto>(
        IQueryable<T> source,
        Expression<Func<T, TDto>> map,
        PagingOptionsRequest request,
        bool all,
        CancellationToken ct)
    {
        var count = await source.CountAsync(ct);

        if (count == 0) return new PaginatedList<TDto>();

        var items = await request
            .Handle(source, all)
            .Select(map)
            .ToListAsync(ct);

        return new PaginatedList<TDto>(items, request.GetPageInfo(count));
    }

    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source,
        PagingOptionsRequest request,
        bool all,
        CancellationToken ct)
    {
        var count = await source.CountAsync(ct);

        if (count == 0) return new PaginatedList<T>();

        var items = await request
            .Handle(source, all)
            .ToListAsync(ct);

        return new PaginatedList<T>(items, request.GetPageInfo(count));
    }
}
