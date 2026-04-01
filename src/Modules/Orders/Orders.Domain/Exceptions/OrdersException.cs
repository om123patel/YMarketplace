using Shared.Domain.Exceptions;

namespace Orders.Domain.Exceptions
{
    public class OrdersException : DomainException
    {
        public OrdersException(string code, string message)
            : base(code, message) { }
    }
}