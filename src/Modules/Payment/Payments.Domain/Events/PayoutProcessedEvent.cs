using Shared.Domain.Abstractions;

namespace Payments.Domain.Events
{
    public sealed class PayoutProcessedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid PayoutId { get; }
        public Guid SellerId { get; }
        public decimal Amount { get; }
        public string CurrencyCode { get; }

        public PayoutProcessedEvent(
            Guid payoutId, Guid sellerId,
            decimal amount, string currencyCode)
        {
            PayoutId = payoutId;
            SellerId = sellerId;
            Amount = amount;
            CurrencyCode = currencyCode;
        }
    }
}