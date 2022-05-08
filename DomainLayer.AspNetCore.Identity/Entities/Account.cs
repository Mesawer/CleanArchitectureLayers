using Mesawer.DomainLayer.AspNetCore.Identity.Enums;
using Mesawer.DomainLayer.Models;
using Mesawer.DomainLayer.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Mesawer.DomainLayer.AspNetCore.Identity.Entities;

public class Account<TUser> : IdentityUserRole<string> where TUser : ApplicationUser
{
    public string Id { get; set; }

    public FullName Name { get; set; }

    public AccountStatus Status { get; set; }

    public Language Language { get; set; }

    public TUser User { get; set; }
}
