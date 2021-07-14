using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Mesawer.ApplicationLayer.Exceptions
{
    /// <summary>
    /// An exception that should be thrown if request's values are semantically valid but not correct.
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string error) => Error = error;

        public BadRequestException(IEnumerable<IdentityError> errors) :
            this(errors.Select(e => e.Description).First()) { }

        public string Error { get; }
    }
}
