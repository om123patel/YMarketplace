using Shared.Domain.Abstractions;

namespace Catalog.Domain.Entities
{
    public class ProductSeo : Entity<int>
    {
        public Guid ProductId { get; private set; }
        public string? MetaTitle { get; private set; }
        public string? MetaDescription { get; private set; }
        public string? MetaKeywords { get; private set; }
        public string? CanonicalUrl { get; private set; }
        public string? OgTitle { get; private set; }
        public string? OgDescription { get; private set; }
        public string? OgImageUrl { get; private set; }

        private ProductSeo() { }  // EF Core

        public static ProductSeo Create(Guid productId)
        {
            return new ProductSeo
            {
                ProductId = productId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };
        }

        public void Update(
            Guid updatedBy,
            string? metaTitle = null,
            string? metaDescription = null,
            string? metaKeywords = null,
            string? canonicalUrl = null,
            string? ogTitle = null,
            string? ogDescription = null,
            string? ogImageUrl = null)
        {
            MetaTitle = metaTitle;
            MetaDescription = metaDescription;
            MetaKeywords = metaKeywords;
            CanonicalUrl = canonicalUrl;
            OgTitle = ogTitle;
            OgDescription = ogDescription;
            OgImageUrl = ogImageUrl;
            SetUpdatedBy(updatedBy);
        }
    }

}
