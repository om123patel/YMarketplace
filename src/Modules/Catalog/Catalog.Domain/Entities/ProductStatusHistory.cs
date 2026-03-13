using Catalog.Domain.Enums;
using Shared.Domain.Abstractions;

namespace Catalog.Domain.Entities
{
    public class ProductStatusHistory : Entity<int>
    {
        public Guid ProductId { get; private set; }
        public ProductStatus? FromStatus { get; private set; }
        public ProductStatus ToStatus { get; private set; }
        public string? Note { get; private set; }
        public Guid ChangedBy { get; private set; }
        public DateTime ChangedAt { get; private set; }

        private ProductStatusHistory() { }  // EF Core

        public static ProductStatusHistory Create(
            Guid productId,
            ProductStatus? fromStatus,
            ProductStatus toStatus,
            Guid changedBy,
            string? note = null)
        {
            return new ProductStatusHistory
            {
                ProductId = productId,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ChangedBy = changedBy,
                Note = note,
                ChangedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = changedBy
            };
        }
    }

}
