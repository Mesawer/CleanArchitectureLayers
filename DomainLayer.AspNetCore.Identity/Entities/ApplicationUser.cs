using System;
using System.Collections.Generic;
using Mesawer.DomainLayer.Models;
using Microsoft.AspNetCore.Identity;

namespace Mesawer.DomainLayer.AspNetCore.Identity.Entities
{
    public class ApplicationUser : IdentityUser, IHasDomainEvent
    {
        public DateTimeOffset CreatedAt { get; set; }

        public List<DomainEvent> DomainEvents { get; } = new();
    }
}
