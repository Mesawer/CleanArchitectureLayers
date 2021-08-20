using Mesawer.DomainLayer.CustomIdentity.Enums;

namespace Mesawer.DomainLayer.CustomIdentity.Entities
{
    public class SocialLogin
    {
        public LoginProvider LoginProvider { get; set; }

        public string ProviderKey { get; set; }

        public string UserId { get; set; }
    }
}
