using Shared.Domain.Abstractions;

namespace Orders.Domain.Events
{
    public sealed class OrderCancelledEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public Guid OrderId { get; }
        public Guid BuyerId { get; }
        public decimal RefundAmount { get; }
        public string CurrencyCode { get; }
        public string Reason { get; }

        public OrderCancelledEvent(
            Guid orderId,
            Guid buyerId,
            decimal refundAmount,
            string currencyCode,
            string reason)
        {
            OrderId = orderId;
            BuyerId = buyerId;
            RefundAmount = refundAmount;
            CurrencyCode = currencyCode;
            Reason = reason;
        }
    }
}