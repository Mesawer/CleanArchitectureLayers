using JetBrains.Annotations;

namespace Mesawer.DomainLayer.Exceptions;

[PublicAPI]
public class InvalidValueException : DomainException
{
    public InvalidValueException()
        : base("The value can't be empty.") { }

    public InvalidValueException(string value)
        : base($"The value \"{value}\" is invalid.") { }
}
