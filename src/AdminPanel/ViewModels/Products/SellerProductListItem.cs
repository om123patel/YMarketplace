namespace AdminPanel.ViewModels.Products
{
    public class SellerProductListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public string Status { get; set; } = string.Empty;
        public string? SellerName { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public int VariantCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
