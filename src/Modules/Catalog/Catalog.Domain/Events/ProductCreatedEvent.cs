using Shared.Domain.Abstractions;

namespace Catalog.Domain.Events
{
    public sealed class ProductCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid ProductId { get; }
        public string ProductName { get; }
        public Guid? SellerId { get; }

        public ProductCreatedEvent(Guid productId, string productName, Guid? sellerId)
        {
            ProductId = productId;
            ProductName = productName;
            SellerId = sellerId;
        }
    }
}
