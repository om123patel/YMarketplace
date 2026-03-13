using Shared.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Domain.Entities
{
    public class ProductImage : Entity<int>
    {
        public Guid ProductId { get; private set; }
        public string ImageUrl { get; private set; } = string.Empty;
        public string? ThumbnailUrl { get; private set; }
        public string? AltText { get; private set; }
        public int SortOrder { get; private set; }
        public bool IsPrimary { get; private set; }

        private ProductImage() { }  // EF Core

        public static ProductImage Create(
            Guid productId,
            string imageUrl,
            Guid createdBy,
            string? thumbnailUrl = null,
            string? altText = null,
            int sortOrder = 0,
            bool isPrimary = false)
        {
            return new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                ThumbnailUrl = thumbnailUrl,
                AltText = altText,
                SortOrder = sortOrder,
                IsPrimary = isPrimary,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }
        // Add to ProductImage.cs
        public void SetSortOrder(int sortOrder)
        {
            SortOrder = sortOrder;
        }
        public void SetAsPrimary() => IsPrimary = true;
        public void UnsetAsPrimary() => IsPrimary = false;
    }

}
