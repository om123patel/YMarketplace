

namespace AdminPanel.ViewModels.Products
{
    // ── List item (table row) ────────────────────────────────────
    public class ProductListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public decimal BasePrice { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public string? SellerName { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public int VariantCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Admin product list ───────────────────────────────────────
    public class ProductListViewModel
    {
        public List<ProductListItem> Items { get; set; } = [];

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        // Filters
        public string? Search { get; set; }
        public string? Status { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string? CreatorType { get; set; }  // "Admin" | "Seller"

        // Sorting
        public string SortBy { get; set; } = "createdat";
        public string SortDirection { get; set; } = "desc";

        // For dropdowns in filter bar
        public List<SelectItem> Categories { get; set; } = [];
        public List<SelectItem> Brands { get; set; } = [];
    }

    // ── Seller product list (simpler — no CreatorType filter) ───
    public class SellerProductListViewModel
    {
        public List<ProductListItem> Items { get; set; } = [];

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        public string? Search { get; set; }
        public string? Status { get; set; }
        public int? CategoryId { get; set; }
        public string SortBy { get; set; } = "createdat";
        public string SortDirection { get; set; } = "desc";

        public List<SelectItem> Categories { get; set; } = [];
    }

    // ── Detail view ──────────────────────────────────────────────
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
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
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

        // Children
        public List<ProductDetailImageItem> Images { get; set; } = [];
        public List<ProductDetailVariantItem> Variants { get; set; } = [];
        public List<ProductDetailTagItem> Tags { get; set; } = [];

        // ── Computed helpers ───────────────────────────────────
        public string? PrimaryImageUrl =>
            Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
            ?? Images.FirstOrDefault()?.ImageUrl;

        public bool IsSellerProduct => CreatorType == "Seller" || SellerId.HasValue;

        public bool CanApprove => Status == "PendingApproval";
        public bool CanReject => Status == "PendingApproval";
        public bool CanArchive => Status != "Archived";
        public bool CanEdit => Status != "Archived";
        public bool CanDelete => true;
    }

    public class ProductDetailImageItem
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;  // ImageUrl not Url
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
        public int StockQuantity { get; set; }   // 0 until Inventory module exists
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

    public class ProductImageItem
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProductVariantItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Sku { get; set; }
        public int StockQuantity { get; set; }
    }

    // ── Edit view (reuses ProductFormViewModel for the form itself) ──
    // The controller loads ProductDto and maps it into ProductFormViewModel.
    // This thin wrapper adds the product ID and original name for the
    // breadcrumb / page title.
    public class ProductEditViewModel
    {
        public Guid Id { get; set; }
        public string OriginalName { get; set; } = string.Empty;

        // The full form data (step wizard reuses Form.cshtml)
        public CreateProductViewModel Form { get; set; } = new();
    }

    // ── Shared helpers ───────────────────────────────────────────
    public class SelectItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Selected { get; set; }

    }
}
