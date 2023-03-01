using System;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Exceptions;

/// <summary>
/// An exception that should be thrown if an unhandled exception has been thrown.
/// </summary>
[PublicAPI]
public class UnhandledRequestException : Exception
{
    public UnhandledRequestException(string msg, Exception inter) : base(msg, inter) { }
}
