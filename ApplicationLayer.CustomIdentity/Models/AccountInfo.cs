using System;
using Mesawer.DomainLayer.ValueObjects;

namespace Mesawer.ApplicationLayer.CustomIdentity.Models
{
    public class AccountInfo<TRole> where TRole : Enum
    {
        public FullName Name { get; set; }

        public TRole Role { get; set; }
    }
}
