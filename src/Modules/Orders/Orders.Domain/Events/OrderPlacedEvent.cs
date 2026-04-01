using Shared.Domain.Abstractions;

namespace Orders.Domain.Events
{
    public sealed class OrderPlacedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public Guid OrderId { get; }
        public Guid BuyerId { get; }
        public Guid StoreId { get; }
        public IReadOnlyList<OrderPlacedItem> Items { get; }
        public decimal TotalAmount { get; }
        public string CurrencyCode { get; }

        public OrderPlacedEvent(
            Guid orderId,
            Guid buyerId,
            Guid storeId,
            IReadOnlyList<OrderPlacedItem> items,
            decimal totalAmount,
            string currencyCode)
        {
            OrderId = orderId;
            BuyerId = buyerId;
            StoreId = storeId;
            Items = items;
            TotalAmount = totalAmount;
            CurrencyCode = currencyCode;
        }
    }

    public record OrderPlacedItem(Guid ProductId, Guid? VariantId, int Quantity, decimal UnitPrice);
}