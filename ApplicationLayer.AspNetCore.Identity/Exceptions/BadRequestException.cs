using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Mesawer.ApplicationLayer.AspNetCore.Identity.Exceptions
{
    public class BadRequestException : ApplicationLayer.Exceptions.BadRequestException
    {
        public BadRequestException(string message) : base(message) { }

        public BadRequestException(IdentityResult identityResult) :
            base(identityResult.Errors.FirstOrDefault()?.Description) { }
    }
}
