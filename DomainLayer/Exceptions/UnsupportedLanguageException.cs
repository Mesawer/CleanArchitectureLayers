using JetBrains.Annotations;

namespace Mesawer.DomainLayer.Exceptions;

[PublicAPI]
public class UnsupportedLanguageException : DomainException
{
    public UnsupportedLanguageException(string lang)
        : base($"Language \"{lang}\" is unsupported.") { }
}
