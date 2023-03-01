using System;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Exceptions;

[PublicAPI]
public class ConcurrencyException : Exception
{
    public ConcurrencyException(InvalidOperationException ex) : base("Can't commit transaction", ex) { }
}
