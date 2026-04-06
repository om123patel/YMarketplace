using Shared.Application.Models;

namespace Payments.Application.DTOs.Transactions
{
    public class TransactionFilterRequest : PagedRequest
    {
        public string? Status { get; set; }
        public string? Method { get; set; }
        public Guid? SellerId { get; set; }
        public Guid? BuyerId { get; set; }
        public Guid? OrderId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}