namespace AdminPanel.Dtos.Inventory
{
    public class AdjustStockRequest
    {
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateStockSettingsRequest
    {
        public int LowStockThreshold { get; set; }
        public bool TrackInventory { get; set; }
        public bool AllowBackorder { get; set; }
    }

    public class CreateStockRequest
    {
        public Guid? VariantId { get; set; }
        public int InitialQuantity { get; set; }
    }
}