using Shared.Domain.Abstractions;

namespace Payments.Domain.Events
{
    public sealed class PaymentRefundedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid TransactionId { get; }
        public Guid OrderId { get; }
        public decimal RefundAmount { get; }
        public string CurrencyCode { get; }

        public PaymentRefundedEvent(
            Guid transactionId, Guid orderId,
            decimal refundAmount, string currencyCode)
        {
            TransactionId = transactionId;
            OrderId = orderId;
            RefundAmount = refundAmount;
            CurrencyCode = currencyCode;
        }
    }
}