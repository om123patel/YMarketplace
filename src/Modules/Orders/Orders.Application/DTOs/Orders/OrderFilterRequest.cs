using Shared.Application.Models;

namespace Orders.Application.DTOs.Orders
{
    public class OrderFilterRequest : PagedRequest
    {
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public Guid? BuyerId { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? SellerId { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? Search { get; set; } // order number search
    }
}
