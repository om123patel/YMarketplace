using Shared.Domain.Abstractions;
using Shared.Domain.Primitives;

namespace Catalog.Domain.Entities
{
    public class ProductVariant : Entity<Guid>, IConcurrencyToken
    {
        public Guid ProductId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Sku { get; private set; }
        public string? Barcode { get; private set; }
        public Money Price { get; private set; } = default!;
        public Money? CompareAtPrice { get; private set; }
        public Money? CostPrice { get; private set; }
        public decimal? WeightKg { get; private set; }
        public string? ImageUrl { get; private set; }
        public int SortOrder { get; private set; }
        public bool IsActive { get; private set; }

        // IConcurrencyToken
        public byte[]? RowVersion { get; private set; }

        // Navigation
        public ICollection<ProductAttribute> Attributes { get; private set; } = [];

        private ProductVariant() { }

        public static ProductVariant Create(
            Guid productId,
            string name,
            Money price,
            Guid createdBy,
            string? sku = null,
            string? barcode = null,
            Money? compareAtPrice = null,
            Money? costPrice = null,
            decimal? weightKg = null,
            string? imageUrl = null,
            int sortOrder = 0)
        {
            return new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Name = name,
                Price = price,
                Sku = sku,
                Barcode = barcode,
                CompareAtPrice = compareAtPrice,
                CostPrice = costPrice,
                WeightKg = weightKg,
                ImageUrl = imageUrl,
                SortOrder = sortOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void AddAttribute(string name, string value, int sortOrder = 0)
        {
            var attribute = ProductAttribute.Create(Id, name, value, sortOrder);
            Attributes.Add(attribute);
        }

        public void Update(
            string name,
            Money price,
            Guid updatedBy,
            string? sku = null,
            Money? compareAtPrice = null,
            decimal? weightKg = null,
            string? imageUrl = null)
        {
            Name = name;
            Price = price;
            Sku = sku;
            CompareAtPrice = compareAtPrice;
            WeightKg = weightKg;
            ImageUrl = imageUrl;
            SetUpdatedBy(updatedBy);
        }

        public void Activate(Guid updatedBy) { IsActive = true; SetUpdatedBy(updatedBy); }
        public void Deactivate(Guid updatedBy) { IsActive = false; SetUpdatedBy(updatedBy); }
    }

}
