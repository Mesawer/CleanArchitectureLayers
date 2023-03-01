using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Models;

[PublicAPI]
public class Localizable<T>
{
    public Localizable(T key, LocalizedStringDto s)
    {
        Key    = key;
        String = s;
    }

    public T Key { get; set; }
    public LocalizedStringDto String { get; set; }
}
