// src/Modules/Catalog/Catalog.Domain/Entities/Wishlist.cs

using Shared.Domain.Abstractions;

namespace Catalog.Domain.Entities
{
    public class Wishlist : AggregateRoot<Guid>
    {
        public Guid BuyerId { get; private set; }
        public ICollection<WishlistItem> Items { get; private set; } = [];

        private Wishlist() { } // EF Core

        public static Wishlist Create(Guid buyerId, Guid createdBy)
        {
            return new Wishlist
            {
                Id = Guid.NewGuid(),
                BuyerId = buyerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public WishlistItem AddItem(Guid productId, Guid createdBy)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing is not null)
                return existing; // idempotent — already in wishlist

            var item = WishlistItem.Create(Id, productId, createdBy);
            Items.Add(item);
            SetUpdatedBy(createdBy);
            return item;
        }

        public void RemoveItem(Guid productId, Guid updatedBy)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item is null) return; // idempotent

            Items.Remove(item);
            SetUpdatedBy(updatedBy);
        }

        public bool Contains(Guid productId)
            => Items.Any(i => i.ProductId == productId);
    }
}