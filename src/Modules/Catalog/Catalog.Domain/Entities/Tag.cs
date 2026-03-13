using Shared.Domain.Abstractions;

namespace Catalog.Domain.Entities
{
    public class Tag : Entity<int>
    {
        public string Name { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;

        public ICollection<Product> Products { get; private set; } = [];

        private Tag() { }  // EF Core

        public static Tag Create(string name, string slug, Guid createdBy)
        {
            return new Tag
            {
                Name = name,
                Slug = slug.ToLowerInvariant(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void Update(string name, string slug, Guid updatedBy)
        {
            Name = name;
            Slug = slug.ToLowerInvariant();
            SetUpdatedBy(updatedBy);
        }
    }

}
