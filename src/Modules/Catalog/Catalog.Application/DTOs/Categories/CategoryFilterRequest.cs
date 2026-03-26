using Shared.Application.Models;

namespace Catalog.Application.DTOs.Categories
{
    public class CategoryFilterRequest : PagedRequest
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
    }
}
