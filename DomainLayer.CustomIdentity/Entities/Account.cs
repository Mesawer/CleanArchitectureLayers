using System;
using Mesawer.DomainLayer.CustomIdentity.Enums;
using Mesawer.DomainLayer.Models;
using Mesawer.DomainLayer.ValueObjects;

namespace Mesawer.DomainLayer.CustomIdentity.Entities
{
    public class Account<TUser, TRole> where TUser : ApplicationUser where TRole : Enum
    {
        public string Id { get; set; }

        public FullName Name { get; set; }

        public AccountStatus Status { get; set; }

        public Language Language { get; set; }

        public TRole Role { get; set; }

        public string UserId { get; set; }
        public TUser User { get; set; }
    }
}
