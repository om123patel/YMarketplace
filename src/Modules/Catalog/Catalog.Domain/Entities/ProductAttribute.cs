using Shared.Domain.Abstractions;

namespace Catalog.Domain.Entities
{
    public class ProductAttribute : Entity<int>
    {
        public Guid VariantId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Value { get; private set; } = string.Empty;
        public int SortOrder { get; private set; }

        private ProductAttribute() { }  // EF Core

        public static ProductAttribute Create(
            Guid variantId,
            string name,
            string value,
            int sortOrder = 0)
        {
            return new ProductAttribute
            {
                VariantId = variantId,
                Name = name,
                Value = value,
                SortOrder = sortOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty  // set by caller
            };
        }

        public void Update(string value, Guid updatedBy)
        {
            Value = value;
            SetUpdatedBy(updatedBy);
        }
    }

}
