using Shared.Application.Models;

namespace Catalog.Application.DTOs.Brands
{
    public class BrandFilterRequest : PagedRequest
    {
        public string? Search { get; set; }
    }
}
