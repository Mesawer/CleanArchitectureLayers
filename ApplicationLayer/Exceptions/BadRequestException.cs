using System;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Exceptions;

/// <summary>
/// An exception that should be thrown if request's values are semantically valid but not correct.
/// </summary>
[PublicAPI]
public class BadRequestException : Exception
{
    public BadRequestException(string title, string error) : base(title) => Error = error;

    public BadRequestException(string error) => Error = error;

    public BadRequestException(string error, Exception innerException) : base(error, innerException) => Error = error;

    public string Error { get; }
}
