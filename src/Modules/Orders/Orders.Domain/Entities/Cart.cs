using Orders.Domain.Exceptions;
using Shared.Domain.Abstractions;

namespace Orders.Domain.Entities
{
    public class Cart : AggregateRoot<Guid>
    {
        public Guid BuyerId { get; private set; }
        public ICollection<CartItem> Items { get; private set; } = [];

        public decimal TotalAmount => Items.Sum(i => i.LineTotal);
        public int TotalItems => Items.Sum(i => i.Quantity);
        public bool IsEmpty => !Items.Any();

        private Cart() { } // EF Core

        public static Cart Create(Guid buyerId, Guid createdBy)
        {
            return new Cart
            {
                Id = Guid.NewGuid(),
                BuyerId = buyerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public CartItem AddItem(
            Guid productId,
            Guid? variantId,
            string productName,
            string? variantName,
            string? imageUrl,
            decimal unitPrice,
            string currencyCode,
            int quantity,
            Guid updatedBy)
        {
            // If the same product+variant already exists, increase quantity
            var existing = Items.FirstOrDefault(i =>
                i.ProductId == productId && i.VariantId == variantId);

            if (existing is not null)
            {
                existing.UpdateQuantity(existing.Quantity + quantity, updatedBy);
                SetUpdatedBy(updatedBy);
                return existing;
            }

            var item = CartItem.Create(
                Id, productId, variantId, productName, variantName,
                imageUrl, unitPrice, currencyCode, quantity, updatedBy);

            Items.Add(item);
            SetUpdatedBy(updatedBy);
            return item;
        }

        public void UpdateItemQuantity(Guid cartItemId, int quantity, Guid updatedBy)
        {
            var item = Items.FirstOrDefault(i => i.Id == cartItemId)
                ?? throw new OrdersException("CART_ITEM_NOT_FOUND",
                    $"Cart item '{cartItemId}' not found.");

            item.UpdateQuantity(quantity, updatedBy);
            SetUpdatedBy(updatedBy);
        }

        public void RemoveItem(Guid cartItemId, Guid updatedBy)
        {
            var item = Items.FirstOrDefault(i => i.Id == cartItemId)
                ?? throw new OrdersException("CART_ITEM_NOT_FOUND",
                    $"Cart item '{cartItemId}' not found.");

            Items.Remove(item);
            SetUpdatedBy(updatedBy);
        }

        public void Clear(Guid updatedBy)
        {
            Items.Clear();
            SetUpdatedBy(updatedBy);
        }
    }
}