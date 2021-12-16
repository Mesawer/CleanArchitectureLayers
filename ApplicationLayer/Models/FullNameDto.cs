using Mesawer.DomainLayer.ValueObjects;
using static Mesawer.ApplicationLayer.Constants;

namespace Mesawer.ApplicationLayer.Models
{
    public class FullNameDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public FullName ToFullName()
        {
            var firstName = FirstName.Length >= NameMaxLength
                ? FirstName[..NameMaxLength]
                : FirstName;

            var lastName = LastName.Length >= NameMaxLength
                ? LastName[..NameMaxLength]
                : LastName;

            return (FullName) (firstName, lastName);
        }

        public static FullNameDto Create(FullName source)
            => new()
            {
                FirstName = source.FirstName,
                LastName  = source.LastName
            };
    }
}
