using System;

namespace Mesawer.ApplicationLayer.Exceptions
{
    /// <summary>
    /// An exception that should be thrown if request's values are semantically valid but not correct.
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string error) => Error = error;

        public string Error { get; }
    }
}
