using Shared.Application.Models;

namespace Catalog.Application.DTOs.Attributes
{
    public class AttributeFilterRequest : PagedRequest
    {
        public string? Search { get; set; }
    }
}
