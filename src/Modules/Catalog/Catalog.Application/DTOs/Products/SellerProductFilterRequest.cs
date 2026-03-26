using Shared.Application.Models;

namespace Catalog.Application.DTOs.Products
{
    public class SellerProductFilterRequest : PagedRequest
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public bool? IsFeatured { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? SortBy { get; set; }
        public string SortDirection { get; set; } = "desc";
    }
}
