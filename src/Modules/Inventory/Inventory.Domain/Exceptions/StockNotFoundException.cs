namespace Inventory.Domain.Exceptions
{
    public class StockNotFoundException : InventoryException
    {
        public StockNotFoundException(Guid productId)
            : base("STOCK_NOT_FOUND",
                   $"Stock record for product '{productId}' was not found.")
        { }
    }
}