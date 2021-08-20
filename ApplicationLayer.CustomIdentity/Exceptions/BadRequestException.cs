using System.Linq;
using Mesawer.ApplicationLayer.CustomIdentity.Models;

namespace Mesawer.ApplicationLayer.CustomIdentity.Exceptions
{
    public class BadRequestException : ApplicationLayer.Exceptions.BadRequestException
    {
        public BadRequestException(IdentityResult identityResult) :
            base(identityResult.Errors.FirstOrDefault()) { }
    }
}
