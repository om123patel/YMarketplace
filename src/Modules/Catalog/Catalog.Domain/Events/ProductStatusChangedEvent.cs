using Catalog.Domain.Enums;
using Shared.Domain.Abstractions;

namespace Catalog.Domain.Events
{
    public sealed class ProductStatusChangedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid ProductId { get; }
        public ProductStatus FromStatus { get; }
        public ProductStatus ToStatus { get; }
        public Guid ChangedBy { get; }

        public ProductStatusChangedEvent(
            Guid productId,
            ProductStatus fromStatus,
            ProductStatus toStatus,
            Guid changedBy)
        {
            ProductId = productId;
            FromStatus = fromStatus;
            ToStatus = toStatus;
            ChangedBy = changedBy;
        }
    }
}
