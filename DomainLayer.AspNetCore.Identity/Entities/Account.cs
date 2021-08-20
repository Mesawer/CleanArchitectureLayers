using Mesawer.DomainLayer.AspNetCore.Identity.Enums;
using Mesawer.DomainLayer.Entities;
using Mesawer.DomainLayer.Models;
using Mesawer.DomainLayer.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Mesawer.DomainLayer.AspNetCore.Identity.Entities
{
    public class Account<TUser, TSession> : IdentityUserRole<string>
        where TUser : ApplicationUser where TSession : Session
    {
        public FullName Name { get; set; }

        public AccountStatus Status { get; set; }

        public Language Language { get; set; }

        public TUser User { get; set; }
        public TSession Session { get; set; }
    }
}
