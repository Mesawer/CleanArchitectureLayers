using System;

namespace Mesawer.PresentationLayer.Models;

public class HttpResult<T> where T : IConvertible
{
    public T Result { get; set; }

    public static HttpResult<T> Create(T result)
        => new()
        {
            Result = result
        };
}
