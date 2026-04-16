// src/Modules/Catalog/Catalog.Domain/Entities/WishlistItem.cs

using Shared.Domain.Abstractions;

namespace Catalog.Domain.Entities
{
    public class WishlistItem : Entity<Guid>
    {
        public Guid WishlistId { get; private set; }
        public Guid ProductId { get; private set; }

        private WishlistItem() { } // EF Core

        public static WishlistItem Create(Guid wishlistId, Guid productId, Guid createdBy)
        {
            return new WishlistItem
            {
                Id = Guid.NewGuid(),
                WishlistId = wishlistId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }
    }
}