namespace Catalog.Application.DTOs.Products
{
    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public decimal? CompareAtPrice { get; set; }
        public decimal? WeightKg { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public List<ProductAttributeDto> Attributes { get; set; } = [];
    }


}
