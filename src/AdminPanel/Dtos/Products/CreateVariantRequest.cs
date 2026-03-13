namespace AdminPanel.Dtos.Products
{
    public class CreateVariantRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public decimal? CompareAtPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? WeightKg { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
        public List<VariantAttributeRequest> Attributes { get; set; } = [];
    }
}
