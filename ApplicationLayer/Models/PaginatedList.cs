using System.Collections.Generic;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Models;

[PublicAPI]
public class PaginatedList<T>
{
    public PageInfo PageInfo { get; }

    public List<T> Items { get; }

    public PaginatedList()
    {
        PageInfo = PageInfo.Empty();
        Items    = new List<T>();
    }

    public PaginatedList(List<T> items, PageInfo pageInfo)
    {
        PageInfo = pageInfo;
        Items    = items;
    }
}
