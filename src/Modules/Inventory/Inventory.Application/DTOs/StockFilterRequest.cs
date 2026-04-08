using Shared.Application.Models;

namespace Inventory.Application.DTOs
{
    public class StockFilterRequest : PagedRequest
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public bool? LowStockOnly { get; set; }
        public Guid? SellerId { get; set; }
    }
}