namespace AdminPanel.ViewModels.Inventory
{
    public class AdjustStockViewModel
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }
}