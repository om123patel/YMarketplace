using Shared.Domain.Abstractions;

namespace Catalog.Domain.Entities
{
    public class Brand : Entity<int>, IConcurrencyToken
    {
        public string Name { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string? LogoUrl { get; private set; }
        public string? WebsiteUrl { get; private set; }
        public bool IsActive { get; private set; }

        // IConcurrencyToken
        public byte[]? RowVersion { get; private set; }

        // Navigation
        public ICollection<Product> Products { get; private set; } = [];

        private Brand() { }

        public static Brand Create(
            string name,
            string slug,
            Guid createdBy,
            string? description = null,
            string? logoUrl = null,
            string? websiteUrl = null)
        {
            return new Brand
            {
                Name = name,
                Slug = slug.ToLowerInvariant(),
                Description = description,
                LogoUrl = logoUrl,
                WebsiteUrl = websiteUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void Update(
            string name,
            string slug,
            Guid updatedBy,
            string? description = null,
            string? logoUrl = null,
            string? websiteUrl = null)
        {
            Name = name;
            Slug = slug.ToLowerInvariant();
            Description = description;
            LogoUrl = logoUrl;
            WebsiteUrl = websiteUrl;
            SetUpdatedBy(updatedBy);
        }

        public void Activate(Guid updatedBy) { IsActive = true; SetUpdatedBy(updatedBy); }
        public void Deactivate(Guid updatedBy) { IsActive = false; SetUpdatedBy(updatedBy); }
    }


}
