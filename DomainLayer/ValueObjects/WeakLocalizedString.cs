using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Mesawer.DomainLayer.Exceptions;
using Mesawer.DomainLayer.Models;
using AmbiguousMatchException = System.Reflection.AmbiguousMatchException;

namespace Mesawer.DomainLayer.ValueObjects;

public class WeakLocalizedString : ValueObject
{
    protected WeakLocalizedString() { }

    public virtual string Ar { get; set; }
    public virtual string En { get; set; }

    public static implicit operator string(WeakLocalizedString str) => str?.ToString();

    public static explicit operator WeakLocalizedString((string ar, string en) values)
        => new()
        {
            Ar = values.ar,
            En = values.en
        };

    public override string ToString()
    {
        var culture = CultureInfo.CurrentCulture;
        var lang    = culture.IsNeutralCulture ? culture.Name : culture.Parent.Name;

        var property = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(p => p.Name.ToLower() == lang);

        if (property is null) throw new UnsupportedLanguageException(lang);

        if (!property.CanRead || property.PropertyType != typeof(string))
            throw new AmbiguousMatchException("Language match can't be read");

        return property.GetValue(this)?.ToString() ?? Ar;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Ar;
        yield return En;
    }
}
