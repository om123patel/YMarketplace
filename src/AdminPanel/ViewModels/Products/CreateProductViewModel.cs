using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Products
{
    public class ProductFormViewModel
    {
        // ── Mode flags ────────────────────────────────────────────
        public Guid? Id { get; set; }        // null = Create
        public bool IsAdminMode { get; set; }        // set from User.IsInRole("Admin")
        public bool IsEditMode => Id.HasValue;

        // ── Step 1 · Basic Info ───────────────────────────────────
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(300, MinimumLength = 3,
            ErrorMessage = "Name must be between 3 and 300 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$",
            ErrorMessage = "Slug must be lowercase letters, numbers and hyphens only")]
        public string Slug { get; set; } = string.Empty;

        public string? ShortDescription { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        public int? BrandId { get; set; }

        // Tag IDs — checkbox list posted as multiple values
        public List<int> TagIds { get; set; } = [];

        public bool IsDigital { get; set; }

        // ── Step 2 · Pricing ──────────────────────────────────────
        [Required(ErrorMessage = "Selling price is required")]
        [Range(0.01, 9_999_999, ErrorMessage = "Price must be greater than 0")]
        public decimal BasePrice { get; set; }

        public string CurrencyCode { get; set; } = "INR";

        public decimal? CompareAtPrice { get; set; }

        // Present in both CreateProductDto and CreateSellerProductDto
        public decimal? CostPrice { get; set; }

        public string? Sku { get; set; }
        public string? Barcode { get; set; }

        // ── Step 2 · Physical dimensions ─────────────────────────
        public decimal? WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }

        // ── Step 3 · Variants ─────────────────────────────────────
        public List<ProductVariantFormItem> Variants { get; set; } = [];

        // ── Step 4 · SEO ──────────────────────────────────────────
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? CanonicalUrl { get; set; }
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string? OgImageUrl { get; set; }

        // ── Step 5 · Organization (Admin only) ───────────────────
        public bool IsFeatured { get; set; }
        public string Status { get; set; } = "Draft";
        public string? RejectionReason { get; set; }

        // ── Dropdowns (populated by controller, never posted) ─────
        public List<SelectItem> Categories { get; set; } = [];
        public List<SelectItem> Brands { get; set; } = [];
        public List<TagSelectItem> AvailableTags { get; set; } = [];
    }

    // ─────────────────────────────────────────────────────────────
    // Variant nested model
    // ─────────────────────────────────────────────────────────────
    public class ProductVariantFormItem
    {
        // null on new variants; set when editing existing variants
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Variant name is required")]
        public string Name { get; set; } = string.Empty;

        public string? Sku { get; set; }
        public string? Barcode { get; set; }

        [Range(0, 9_999_999, ErrorMessage = "Price must be 0 or greater")]
        public decimal Price { get; set; }

        public string CurrencyCode { get; set; } = "INR";
        public decimal? CompareAtPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? WeightKg { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public List<VariantAttributeItem> Attributes { get; set; } = [];
    }

    // ─────────────────────────────────────────────────────────────
    // Variant attribute row
    // ─────────────────────────────────────────────────────────────
    public class VariantAttributeItem
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────────────────────
    // Shared dropdown items
    // ─────────────────────────────────────────────────────────────
    public class TagSelectItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }

 

}
