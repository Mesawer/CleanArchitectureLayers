using System;
using JetBrains.Annotations;

namespace Mesawer.DomainLayer.Exceptions;

[PublicAPI]
public abstract class DomainException : Exception
{
    protected DomainException() { }
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}
