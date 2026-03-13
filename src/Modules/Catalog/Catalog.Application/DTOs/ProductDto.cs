namespace Catalog.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }

        // Ownership
        public Guid? SellerId { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? CreatedByAdminId { get; set; }
        public string CreatorType { get; set; } = string.Empty;

        // Classification
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }

        // Core
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }

        // Pricing
        public decimal BasePrice { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public decimal? CompareAtPrice { get; set; }

        // Identifiers
        public string? Sku { get; set; }
        public string? Barcode { get; set; }

        // Physical
        public decimal? WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }

        // Flags
        public bool IsDigital { get; set; }
        public bool RequiresShipping { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }

        // Approval workflow
        public string Status { get; set; } = string.Empty;
        public DateTime? SubmittedForApprovalAt { get; set; }
        public Guid? ApprovedByAdminId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? RejectedByAdminId { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        // Children
        public ProductSeoDto? Seo { get; set; }
        public List<ProductImageDto> Images { get; set; } = [];
        public List<ProductVariantDto> Variants { get; set; } = [];
        public List<TagDto> Tags { get; set; } = [];

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }


}
