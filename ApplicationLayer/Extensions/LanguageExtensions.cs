using System;
using System.Linq;
using JetBrains.Annotations;
using Mesawer.DomainLayer.Models;

namespace Mesawer.ApplicationLayer.Extensions;

[PublicAPI]
public static class LanguageExtensions
{
    public static Language ToLang(this string lang)
        => EnumsExtensions.ToArray<Language>()
            .FirstOrDefault(c
                => string.Equals(c.ToString(), lang, StringComparison.InvariantCultureIgnoreCase));
}
