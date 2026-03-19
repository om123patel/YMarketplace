namespace AdminPanel.ViewModels.Products
{
    public class ProductDetailViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string CategoryName { get; set; } = string.Empty;
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
        public string CreatorType { get; set; } = string.Empty;
        public Guid? SellerId { get; set; }
        public string? SellerName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // SEO
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? CanonicalUrl { get; set; }
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string? OgImageUrl { get; set; }

        public List<ProductDetailImageItem> Images { get; set; } = [];
        public List<ProductDetailVariantItem> Variants { get; set; } = [];
        public List<ProductDetailTagItem> Tags { get; set; } = [];

        // Computed
        public string? PrimaryImageUrl =>
            Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
            ?? Images.FirstOrDefault()?.ImageUrl;

        public bool IsSellerProduct => SellerId.HasValue;
        public bool CanApprove => Status == "PendingApproval";
        public bool CanReject => Status == "PendingApproval";
        public bool CanArchive => Status != "Archived";
        public bool CanEdit => Status != "Archived";
        public bool CanDelete => true;
    }

    public class ProductDetailImageItem
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProductDetailVariantItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public decimal? CompareAtPrice { get; set; }
        public bool IsActive { get; set; }
        public int StockQuantity { get; set; }  // 0 until Inventory module
        public List<ProductDetailAttributeItem> Attributes { get; set; } = [];
    }

    public class ProductDetailAttributeItem
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ProductDetailTagItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

}
