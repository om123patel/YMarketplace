namespace AdminPanel.Dtos.Products
{
    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Sku { get; set; }
        public int StockQuantity { get; set; }
    }
}
