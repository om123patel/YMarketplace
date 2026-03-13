using Shared.Domain.Abstractions;

namespace Catalog.Domain.Entities
{
    public class Category : Entity<int>, IConcurrencyToken
    {
        public int? ParentId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string? ImageUrl { get; private set; }
        public string? IconUrl { get; private set; }
        public int SortOrder { get; private set; }
        public bool IsActive { get; private set; }

        // IConcurrencyToken
        public byte[]? RowVersion { get; private set; }

        // Navigation
        public Category? Parent { get; private set; }
        public ICollection<Category> Children { get; private set; } = [];
        public ICollection<Product> Products { get; private set; } = [];

        private Category() { }

        public static Category Create(
            string name,
            string slug,
            Guid createdBy,
            int? parentId = null,
            string? description = null,
            string? imageUrl = null,
            string? iconUrl = null,
            int sortOrder = 0)
        {
            var category = new Category
            {
                Name = name,
                Slug = slug.ToLowerInvariant(),
                ParentId = parentId,
                Description = description,
                ImageUrl = imageUrl,
                IconUrl = iconUrl,
                SortOrder = sortOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            return category;
        }

        public void Update(
            string name,
            string slug,
            Guid updatedBy,
            int? parentId = null,
            string? description = null,
            string? imageUrl = null,
            string? iconUrl = null,
            int sortOrder = 0)
        {
            Name = name;
            Slug = slug.ToLowerInvariant();
            ParentId = parentId;
            Description = description;
            ImageUrl = imageUrl;
            IconUrl = iconUrl;
            SortOrder = sortOrder;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate(Guid updatedBy)
        {
            IsActive = true;
            SetUpdatedBy(updatedBy);
        }

        public void Deactivate(Guid updatedBy)
        {
            IsActive = false;
            SetUpdatedBy(updatedBy);
        }
    }

}
