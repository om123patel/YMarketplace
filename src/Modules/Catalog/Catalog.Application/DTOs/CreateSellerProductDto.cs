namespace Catalog.Application.DTOs
{
    public class CreateSellerProductDto
    {
        // Core
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }

        // Classification
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }

        // Pricing
        public decimal BasePrice { get; set; }
        public string CurrencyCode { get; set; } = "USD";
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

        // Related
        public List<CreateProductVariantDto> Variants { get; set; } = [];
        public List<string> ImageUrls { get; set; } = [];
        public List<int> TagIds { get; set; } = [];
        public ProductSeoDto? Seo { get; set; }
    }

}
