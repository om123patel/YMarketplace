namespace Orders.Domain.Exceptions
{
    public class OrderNotFoundException : OrdersException
    {
        public OrderNotFoundException(Guid orderId)
            : base("ORDER_NOT_FOUND", $"Order with id '{orderId}' was not found.") { }
    }
}