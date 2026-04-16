using Shared.Domain.Abstractions;

namespace Payments.Domain.Events
{
    public sealed class PaymentInitiatedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid TransactionId { get; }
        public Guid OrderId { get; }
        public Guid BuyerId { get; }
        public decimal Amount { get; }
        public string CurrencyCode { get; }
        public string GatewayProvider { get; }

        public PaymentInitiatedEvent(
            Guid transactionId, Guid orderId, Guid buyerId,
            decimal amount, string currencyCode, string gatewayProvider)
        {
            TransactionId = transactionId;
            OrderId = orderId;
            BuyerId = buyerId;
            Amount = amount;
            CurrencyCode = currencyCode;
            GatewayProvider = gatewayProvider;
        }
    }
}