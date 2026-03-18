namespace AdminPanel.Dtos.Products
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public decimal BasePrice { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public decimal? CompareAtPrice { get; set; }
        public string? Sku { get; set; }
        public bool IsDigital { get; set; }
        public List<int> TagIds { get; set; } = [];
        public ProductSeoRequest? Seo { get; set; }
        public Guid? SellerId { get; set; }
        public Guid? StoreId { get; set; }
        public List<string> ImageUrls { get; set; } = [];
    }
}
