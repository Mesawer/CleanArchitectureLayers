using Mesawer.ApplicationLayer.Enums;

namespace Mesawer.ApplicationLayer.Models
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
