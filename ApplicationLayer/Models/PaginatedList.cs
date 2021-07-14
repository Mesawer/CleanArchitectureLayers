using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Mesawer.ApplicationLayer.Models
{
    public class PaginatedList<T>
    {
        public PageInfo PageInfo { get; }

        public List<T> Items { get; }

        private PaginatedList()
        {
            PageInfo = PageInfo.Empty();
            Items    = new List<T>();
        }

        public PaginatedList(List<T> items, PageInfo pageInfo)
        {
            PageInfo = pageInfo;
            Items    = items;
        }

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
}
