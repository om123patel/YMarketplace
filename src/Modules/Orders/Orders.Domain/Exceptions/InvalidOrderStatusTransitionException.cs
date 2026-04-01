using Orders.Domain.Enums;

namespace Orders.Domain.Exceptions
{
    public class InvalidOrderStatusTransitionException : OrdersException
    {
        public InvalidOrderStatusTransitionException(OrderStatus from, OrderStatus to)
            : base("INVALID_STATUS_TRANSITION",
                   $"Cannot transition order from '{from}' to '{to}'.")
        { }
    }
}