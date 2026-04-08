namespace AdminPanel.Dtos.Inventory
{
    public class StockListItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public string? ProductName { get; set; }
        public string? VariantName { get; set; }
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int LowStockThreshold { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsLowStock { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}