namespace AdminPanel.Dtos.Products
{
    public class ProductListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public decimal BasePrice { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public string CategoryName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public string CreatorType { get; set; } = string.Empty;
        public string? SellerName { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public int VariantCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
