namespace Mesawer.DomainLayer.Exceptions
{
    public class AmbiguousMatchException : DomainException
    {
        public AmbiguousMatchException(string message) : base(message) { }
    }
}
