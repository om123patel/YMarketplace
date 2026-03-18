using AdminPanel.Dtos.Tags;

namespace AdminPanel.Dtos.Products
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public decimal BasePrice { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public decimal? CompareAtPrice { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal? WeightKg { get; set; }
        public bool IsDigital { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public Guid? SellerId { get; set; }
        public string? SellerName { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public ProductSeoDto? Seo { get; set; }
        public List<ProductImageDto> Images { get; set; } = [];
        public List<ProductVariantDto> Variants { get; set; } = [];
        public List<TagDto> Tags { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductSeoDto
    {
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? CanonicalUrl { get; set; }
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string? OgImageUrl { get; set; }
    }

    //public class TagDto
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; } = string.Empty;
    //    public string Slug { get; set; } = string.Empty;
    //    public int ProductCount { get; set; }
    //}
}
