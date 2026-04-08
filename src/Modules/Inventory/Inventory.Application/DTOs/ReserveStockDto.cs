namespace Inventory.Application.DTOs
{
    public class ReserveStockDto
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public int Quantity { get; set; }
    }
}