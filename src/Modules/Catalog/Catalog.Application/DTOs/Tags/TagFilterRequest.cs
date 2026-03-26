using Shared.Application.Models;

namespace Catalog.Application.DTOs.Tags
{
    public class TagFilterRequest : PagedRequest
    {
        public string? Search { get; set; }
    }
}
