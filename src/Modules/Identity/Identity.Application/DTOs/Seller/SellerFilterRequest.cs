using Shared.Application.Models;

namespace Identity.Application.DTOs.Seller
{
    public class SellerFilterRequest : PagedRequest
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
    }
}
