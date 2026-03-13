

namespace AdminPanel.ViewModels.Products
{
    // ── List item (table row) ────────────────────────────────────
    public class ProductListItem
    {
        public Guid    Id             { get; set; }
        public string  Name           { get; set; } = string.Empty;
        public string  Sku            { get; set; } = string.Empty;
        public string  CategoryName   { get; set; } = string.Empty;
        public string? BrandName      { get; set; }
        public decimal BasePrice      { get; set; }
        public string  CurrencyCode   { get; set; } = "INR";
        public string  Status         { get; set; } = string.Empty;
        public bool    IsActive       { get; set; }
        public bool    IsFeatured     { get; set; }
        public string? SellerName     { get; set; }
        public string? PrimaryImageUrl{ get; set; }
        public int     VariantCount   { get; set; }
        public DateTime CreatedAt     { get; set; }
    }

    // ── Admin product list ───────────────────────────────────────
    public class ProductListViewModel
    {
        public List<ProductListItem> Items { get; set; } = [];

        // Pagination
        public int Page      { get; set; } = 1;
        public int PageSize  { get; set; } = 20;
        public int TotalCount{ get; set; }
        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        // Filters
        public string? Search     { get; set; }
        public string? Status     { get; set; }
        public int?    CategoryId { get; set; }
        public int?    BrandId    { get; set; }
        public string? CreatorType{ get; set; }  // "Admin" | "Seller"

        // Sorting
        public string  SortBy        { get; set; } = "createdat";
        public string  SortDirection { get; set; } = "desc";

        // For dropdowns in filter bar
        public List<SelectItem> Categories { get; set; } = [];
        public List<SelectItem> Brands     { get; set; } = [];
    }

    // ── Seller product list (simpler — no CreatorType filter) ───
    public class SellerProductListViewModel
    {
        public List<ProductListItem> Items { get; set; } = [];

        public int Page      { get; set; } = 1;
        public int PageSize  { get; set; } = 20;
        public int TotalCount{ get; set; }
        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        public string? Search     { get; set; }
        public string? Status     { get; set; }
        public int?    CategoryId { get; set; }
        public string  SortBy        { get; set; } = "createdat";
        public string  SortDirection { get; set; } = "desc";

        public List<SelectItem> Categories { get; set; } = [];
    }

    // ── Detail view ──────────────────────────────────────────────
    public class ProductDetailViewModel
    {
        public Guid    Id              { get; set; }
        public string  Name            { get; set; } = string.Empty;
        public string  Slug            { get; set; } = string.Empty;
        public string? Sku             { get; set; }
        public string? ShortDescription{ get; set; }
        public string? Description     { get; set; }
        public decimal BasePrice       { get; set; }
        public string  CurrencyCode    { get; set; } = "INR";
        public decimal? CompareAtPrice { get; set; }
        public string  Status          { get; set; } = string.Empty;
        public bool    IsActive        { get; set; }
        public bool    IsFeatured      { get; set; }
        public string  CategoryName    { get; set; } = string.Empty;
        public string? BrandName       { get; set; }
        public string? SellerName      { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public DateTime CreatedAt      { get; set; }

        public List<ProductImageItem>   Images   { get; set; } = [];
        public List<ProductVariantItem> Variants { get; set; } = [];

        // Metadata
        public string? MetaTitle       { get; set; }
        public string? MetaDescription { get; set; }

        // For approval flow — only populated in Admin detail
        public bool CanApprove  { get; set; }
        public bool CanReject   { get; set; }
        public bool CanArchive  { get; set; }
        public bool CanDelete   { get; set; }
        public bool CanEdit     { get; set; }
        public bool IsSellerProduct => !string.IsNullOrEmpty(SellerName);
    }

    public class ProductImageItem
    {
        public int    Id       { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool   IsPrimary{ get; set; }
        public int    SortOrder{ get; set; }
    }

    public class ProductVariantItem
    {
        public Guid    Id            { get; set; }
        public string  Name          { get; set; } = string.Empty;
        public decimal Price         { get; set; }
        public string? Sku           { get; set; }
        public int     StockQuantity { get; set; }
    }

    // ── Edit view (reuses ProductFormViewModel for the form itself) ──
    // The controller loads ProductDto and maps it into ProductFormViewModel.
    // This thin wrapper adds the product ID and original name for the
    // breadcrumb / page title.
    public class ProductEditViewModel
    {
        public Guid   Id           { get; set; }
        public string OriginalName { get; set; } = string.Empty;

        // The full form data (step wizard reuses Form.cshtml)
        public ProductFormViewModel Form { get; set; } = new();
    }

    // ── Shared helpers ───────────────────────────────────────────
    public class SelectItem
    {
        public int    Id   { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
