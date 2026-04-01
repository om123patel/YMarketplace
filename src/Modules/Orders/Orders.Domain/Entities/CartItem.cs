using Shared.Domain.Abstractions;

namespace Orders.Domain.Entities
{
    public class CartItem : Entity<Guid>
    {
        public Guid CartId { get; private set; }
        public Guid ProductId { get; private set; }
        public Guid? VariantId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public string? VariantName { get; private set; }
        public string? ProductImageUrl { get; private set; }
        public decimal UnitPrice { get; private set; }
        public string CurrencyCode { get; private set; } = string.Empty;
        public int Quantity { get; private set; }

        private CartItem() { } // EF Core

        public static CartItem Create(
            Guid cartId,
            Guid productId,
            Guid? variantId,
            string productName,
            string? variantName,
            string? productImageUrl,
            decimal unitPrice,
            string currencyCode,
            int quantity,
            Guid createdBy)
        {
            return new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cartId,
                ProductId = productId,
                VariantId = variantId,
                ProductName = productName,
                VariantName = variantName,
                ProductImageUrl = productImageUrl,
                UnitPrice = unitPrice,
                CurrencyCode = currencyCode,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void UpdateQuantity(int quantity, Guid updatedBy)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            Quantity = quantity;
            SetUpdatedBy(updatedBy);
        }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}