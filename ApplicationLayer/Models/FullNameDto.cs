using JetBrains.Annotations;
using Mesawer.DomainLayer.ValueObjects;
using static Mesawer.ApplicationLayer.Constants;

namespace Mesawer.ApplicationLayer.Models;

[PublicAPI]
public class FullNameDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public FullName ToFullName()
    {
        var firstName = FirstName?.Length >= NameMaxLength
            ? FirstName[..NameMaxLength]
            : FirstName;

        var lastName = LastName?.Length >= NameMaxLength
            ? LastName[..NameMaxLength]
            : LastName;

        if (firstName is null || lastName is null) return null;

        return (FullName) (firstName, lastName);
    }

    public static FullNameDto Create(string firstName, string lastName)
        => new()
        {
            FirstName = firstName,
            LastName  = lastName
        };

    public static FullNameDto Create(FullName source)
        => new()
        {
            FirstName = source.FirstName,
            LastName  = source.LastName
        };
}
