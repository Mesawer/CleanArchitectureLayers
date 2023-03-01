using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Mesawer.DomainLayer.Exceptions;

namespace Mesawer.DomainLayer.ValueObjects;

[PublicAPI]
public class LocalizedString : WeakLocalizedString
{
    private LocalizedString()
    {
        _ar = null!;
        _en = null!;
    }

    private string _ar;

    public override string Ar
    {
        get => _ar;
        set
        {
            var str = value?.Trim() ?? throw new InvalidValueException();

            if (!IsValidAr(str)) throw new InvalidValueException(value);

            _ar = value;
        }
    }

    private string _en;

    public override string En
    {
        get => _en;
        set
        {
            var str = value?.Trim() ?? throw new InvalidValueException();

            if (!IsValidEn(str)) throw new InvalidValueException(value);

            _en = value;
        }
    }

    public static implicit operator string(LocalizedString str) => str?.ToString();

    public static explicit operator LocalizedString((string ar, string en) values)
        => new()
        {
            Ar = values.ar,
            En = values.en
        };

    public static bool IsValidAr(string ar)
    {
        var regex = new Regex(
            @"^[\u0600-\u06ff\u0750-\u077f\ufb50-\ufbc1\ufbd3-\ufd3f\ufd50-\ufd8f\ufd92-\ufdc7\ufe70-\ufefc\uFDF0-\uFDFD\s\d\p{P}\p{S}]+$");

        return ar is not null && regex.IsMatch(ar.Trim());
    }

    public static bool IsValidEn(string en)
    {
        var regex = new Regex(@"^[a-zA-Z\s\d\p{P}\p{S}]+$");

        return en is not null && regex.IsMatch(en.Trim());
    }
}
