using Shared.Domain.Exceptions;

namespace Identity.Domain.Exceptions
{
    public class IdentityException : DomainException
    {
        public IdentityException(string code, string message)
            : base(code, message) { }
    }
}
