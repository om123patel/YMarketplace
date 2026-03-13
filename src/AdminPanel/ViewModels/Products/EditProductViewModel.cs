using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Products
{
    public class EditProductViewModel
    {
        public Guid Id { get; set; }

        [Required] public string Name { get; set; } = string.Empty;
        [Required] public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }

        [Required] public int CategoryId { get; set; }
        public int? BrandId { get; set; }

        [Required][Range(0.01, double.MaxValue)] public decimal BasePrice { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public decimal? CompareAtPrice { get; set; }
        public string? Sku { get; set; }
        public bool IsDigital { get; set; }

        // Read-only display info (not posted)
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? SellerName { get; set; }
        public string? BrandName { get; set; }
        public string? CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PrimaryImageUrl { get; set; }

        // For dropdowns
        public List<SelectItem> Categories { get; set; } = [];
        public List<SelectItem> Brands { get; set; } = [];
    }
}
