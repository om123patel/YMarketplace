using Shared.Domain.Abstractions;

namespace Orders.Domain.Events
{
    public sealed class OrderDeliveredEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public Guid OrderId { get; }
        public Guid BuyerId { get; }
        public Guid StoreId { get; }

        public OrderDeliveredEvent(Guid orderId, Guid buyerId, Guid storeId)
        {
            OrderId = orderId;
            BuyerId = buyerId;
            StoreId = storeId;
        }
    }
}