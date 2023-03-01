using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Models;

[PublicAPI]
public class PageInfo
{
    public int PageIndex { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }

    public static PageInfo Empty()
        => new()
        {
            PageIndex  = 1,
            TotalPages = 1,
            TotalCount = 0,
        };
}
