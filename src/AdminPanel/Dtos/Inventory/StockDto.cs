namespace AdminPanel.Dtos.Inventory
{
    public class StockDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int LowStockThreshold { get; set; }
        public bool TrackInventory { get; set; }
        public bool AllowBackorder { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsLowStock { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}