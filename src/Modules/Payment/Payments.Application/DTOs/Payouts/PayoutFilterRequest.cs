using Shared.Application.Models;

namespace Payments.Application.DTOs.Payouts
{
    public class PayoutFilterRequest : PagedRequest
    {
        public string? Status { get; set; }
        public Guid? SellerId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}