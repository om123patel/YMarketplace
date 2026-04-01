using Shared.Domain.Abstractions;

namespace Orders.Domain.Entities
{
    public class OrderItem : Entity<Guid>
    {
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public Guid? VariantId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public string? VariantName { get; private set; }
        public string? ProductImageUrl { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public string CurrencyCode { get; private set; } = string.Empty;
        public decimal LineTotal => UnitPrice * Quantity;

        private OrderItem() { } // EF Core

        public static OrderItem Create(
            Guid orderId,
            Guid productId,
            Guid? variantId,
            string productName,
            string? variantName,
            string? productImageUrl,
            int quantity,
            decimal unitPrice,
            string currencyCode,
            Guid createdBy)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

            return new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = productId,
                VariantId = variantId,
                ProductName = productName,
                VariantName = variantName,
                ProductImageUrl = productImageUrl,
                Quantity = quantity,
                UnitPrice = unitPrice,
                CurrencyCode = currencyCode,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }
    }
}