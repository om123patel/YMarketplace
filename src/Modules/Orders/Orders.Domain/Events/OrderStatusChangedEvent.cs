using Orders.Domain.Enums;
using Shared.Domain.Abstractions;

namespace Orders.Domain.Events
{
    public sealed class OrderStatusChangedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public Guid OrderId { get; }
        public OrderStatus FromStatus { get; }
        public OrderStatus ToStatus { get; }
        public Guid ChangedBy { get; }

        public OrderStatusChangedEvent(
            Guid orderId,
            OrderStatus fromStatus,
            OrderStatus toStatus,
            Guid changedBy)
        {
            OrderId = orderId;
            FromStatus = fromStatus;
            ToStatus = toStatus;
            ChangedBy = changedBy;
        }
    }
}