using AdminPanel.Dtos.Attributes;

namespace AdminPanel.Dtos.Products
{
    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public decimal? CompareAtPrice { get; set; }
        public bool IsActive { get; set; }
        public List<ProductAttributeDto> Attributes { get; set; } = [];
    }

    public class ProductAttributeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
