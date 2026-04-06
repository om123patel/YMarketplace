using Shared.Domain.Exceptions;

namespace Payments.Domain.Exceptions
{
    public class PaymentsException : DomainException
    {
        public PaymentsException(string code, string message)
            : base(code, message) { }
    }
}