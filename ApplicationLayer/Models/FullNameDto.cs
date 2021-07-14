using Mesawer.DomainLayer.ValueObjects;

namespace Mesawer.ApplicationLayer.Models
{
    public class FullNameDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public FullName ToFullName() => (FullName) (FirstName, LastName);

        public static FullNameDto Create(FullName source)
            => new()
            {
                FirstName = source.FirstName,
                LastName  = source.LastName
            };
    }
}
