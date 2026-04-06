using Shared.Domain.Abstractions;

namespace Payments.Domain.Events
{
    public sealed class PaymentCompletedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid TransactionId { get; }
        public Guid OrderId { get; }
        public Guid BuyerId { get; }
        public Guid SellerId { get; }
        public decimal Amount { get; }
        public string CurrencyCode { get; }

        public PaymentCompletedEvent(
            Guid transactionId, Guid orderId, Guid buyerId,
            Guid sellerId, decimal amount, string currencyCode)
        {
            TransactionId = transactionId;
            OrderId = orderId;
            BuyerId = buyerId;
            SellerId = sellerId;
            Amount = amount;
            CurrencyCode = currencyCode;
        }
    }
}