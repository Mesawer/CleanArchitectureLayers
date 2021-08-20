using Mesawer.ApplicationLayer.Models;
using Mesawer.DomainLayer.AspNetCore.Identity.Enums;

namespace Mesawer.ApplicationLayer.AspNetCore.Identity.Models
{
    public class SocialUser
    {
        public LoginProvider Provider { get; set; }

        public string Id { get; set; }

        public string Email { get; set; }

        public FullNameDto FullName { get; set; }

        public string Picture { get; set; }
    }
}
