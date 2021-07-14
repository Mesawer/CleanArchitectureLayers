using System.Collections.Generic;
using Mesawer.DomainLayer.Models;

namespace Mesawer.DomainLayer.ValueObjects
{
    public class FullName : ValueObject
    {
        private FullName() { }

        private FullName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName  = lastName;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public static implicit operator string(FullName fullName) => fullName?.ToString();

        public static explicit operator FullName((string firstName, string lastName) values)
            => new(values.firstName, values.lastName);

        public override string ToString() => FirstName + " " + LastName;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FirstName;
            yield return LastName;
        }
    }
}
