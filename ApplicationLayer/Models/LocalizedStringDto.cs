using System;
using Mesawer.DomainLayer.ValueObjects;

namespace Mesawer.ApplicationLayer.Models;

public class LocalizedStringDto
{
    public string Ar { get; init; }
    public string En { get; init; }

    public LocalizedString ToLocalizedString() => (LocalizedString) (Ar, En);

    public WeakLocalizedString ToWeakLocalizedString() => (WeakLocalizedString) (Ar, En);

    public static LocalizedStringDto Create(LocalizedString source)
        => new()
        {
            Ar = source.Ar,
            En = source.En
        };

    public static LocalizedStringDto Create(WeakLocalizedString source)
        => new()
        {
            Ar = source.Ar,
            En = source.En
        };

    public static bool operator ==(LocalizedString str, LocalizedStringDto dto)
        => str?.Ar == dto?.Ar || str?.En == dto?.En;

    public static bool operator !=(LocalizedString str, LocalizedStringDto dto)
        => str?.Ar != dto?.Ar || str?.En != dto?.En;

    protected bool Equals(LocalizedStringDto other) => Ar == other.Ar || En == other.En;

    public override bool Equals(object obj)
    {
        if (obj is null) return false;

        if (ReferenceEquals(this, obj)) return true;

        return obj.GetType() == GetType() && Equals((LocalizedStringDto) obj);
    }

    public override int GetHashCode() => HashCode.Combine(Ar, En);
}
