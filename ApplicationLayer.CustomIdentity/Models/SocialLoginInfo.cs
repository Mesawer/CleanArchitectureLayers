using Mesawer.DomainLayer.CustomIdentity.Enums;

namespace Mesawer.ApplicationLayer.CustomIdentity.Models
{
    public class SocialLoginInfo
    {
        public LoginProvider LoginProvider { get; set; }

        public string ProviderKey { get; set; }
    }
}
