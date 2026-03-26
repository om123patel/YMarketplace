namespace AdminPanel.ViewModels.Products
{
    public class ProductListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? BrandName { get; set; }

        public decimal BasePrice { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public string Status { get; set; } = string.Empty;
        public string? SellerName { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
        public int VariantCount { get; set; }
        public string? CreatorType { get; set; }
        public DateTime CreatedAt { get; set; }
    }


}
