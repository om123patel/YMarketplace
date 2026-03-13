namespace AdminPanel.Dtos.Products
{
    public class CreateSellerProductRequest
    {
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public decimal? CompareAtPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public bool IsDigital { get; set; }
        public ProductSeoRequest? Seo { get; set; }
        public List<CreateVariantRequest> Variants { get; set; } = [];
        public List<string> ImageUrls { get; set; } = [];
        public List<int> TagIds { get; set; } = [];
    }
}
