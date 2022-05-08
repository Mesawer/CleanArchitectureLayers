using System;
using Mesawer.ApplicationLayer.Resources.Common;

namespace Mesawer.ApplicationLayer.Exceptions;

/// <summary>
/// An exception that should be thrown if the user is trying to access data that isn't found
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException()
        : base(SharedRes.EntityNotFound) { }

    public NotFoundException(string message)
        : base(message) { }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
