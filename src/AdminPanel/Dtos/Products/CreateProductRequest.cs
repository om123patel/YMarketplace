namespace AdminPanel.Dtos.Products
{
    public class CreateProductRequest
    {
        // Ownership — SellerId/StoreId only needed when Admin
        // creates on behalf of a seller; leave null for own products
        public Guid? SellerId { get; set; }
        public Guid? StoreId { get; set; }

        // Classification
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }

        // Core
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }

        // Pricing
        public decimal BasePrice { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public decimal? CompareAtPrice { get; set; }
        public decimal? CostPrice { get; set; }

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

        // SEO
        public ProductSeoRequest? Seo { get; set; }

        // Children
        public List<CreateVariantRequest> Variants { get; set; } = [];
        public List<string> ImageUrls { get; set; } = [];
        public List<int> TagIds { get; set; } = [];
    }
}
