using System;

namespace Mesawer.ApplicationLayer.Exceptions;

/// <summary>
/// An exception that should be thrown if the user is trying to access data that he/she doesn't have permission for.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() { }
}
