namespace AdminPanel.ViewModels.Inventory
{
    public class UpdateStockSettingsViewModel
    {
        public Guid ProductId { get; set; }
        public int LowStockThreshold { get; set; }
        public bool TrackInventory { get; set; }
        public bool AllowBackorder { get; set; }
    }
}