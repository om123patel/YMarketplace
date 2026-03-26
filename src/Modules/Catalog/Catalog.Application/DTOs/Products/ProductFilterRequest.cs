using Shared.Application.Models;

namespace Catalog.Application.DTOs.Products
{
    public class ProductFilterRequest : PagedRequest
    {
        public string? Status { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public Guid? SellerId { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsActive { get; set; }
        public string? CreatorType { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }


}
