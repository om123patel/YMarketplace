namespace Inventory.Application.DTOs
{
    public class UpdateStockSettingsDto
    {
        public int LowStockThreshold { get; set; }
        public bool TrackInventory { get; set; }
        public bool AllowBackorder { get; set; }
    }
}