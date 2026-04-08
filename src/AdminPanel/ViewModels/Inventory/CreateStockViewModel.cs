namespace AdminPanel.ViewModels.Inventory
{
    public class CreateStockViewModel
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public int InitialQuantity { get; set; }
        public int LowStockThreshold { get; set; } = 5;
        public bool TrackInventory { get; set; } = true;
        public bool AllowBackorder { get; set; } = false;
    }
}