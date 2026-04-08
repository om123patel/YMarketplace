namespace AdminPanel.ViewModels.Inventory
{
    public class SetQuantityViewModel
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }
}