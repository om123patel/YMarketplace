using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Products
{
    public class CreateProductViewModel
    {
        // ── Mode ───────────────────────────────────────────────
        // null = Create, has value = Edit
        public Guid? Id { get; set; }

        // Backing field so <input type="hidden" asp-for="IsAdminMode" /> 
        // survives the POST round-trip
        public bool IsAdminMode { get; set; } = true;

        // ── Step 1: Basic Info ─────────────────────────────────
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        [Required] public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public bool IsDigital { get; set; }

        // ── Step 2: Pricing ────────────────────────────────────
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal BasePrice { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public decimal? CompareAtPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }

        // ── Step 3: Variants ───────────────────────────────────
        // Used by _PVariants partial to display/edit existing variants
        private List<ExistingVariantItem> _variants = [];

        public List<ExistingVariantItem> Variants
        {
            get => _variants;
            set => _variants = value;
        }

        public List<ExistingVariantItem> ExistingVariants
        {
            get => _variants;
            set => _variants = value;
        }


        // ── Step 4: SEO ────────────────────────────────────────
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? CanonicalUrl { get; set; }
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string? OgImageUrl { get; set; }

        // ── Step 5: Organisation (admin only) ──────────────────
        public bool IsFeatured { get; set; }
        public string? Status { get; set; }
        public string? RejectionReason { get; set; }

        // Tags — partial uses Model.AvailableTags for the list,
        //        Model.SelectedTagIds for the current selection
        public List<int> SelectedTagIds { get; set; } = [];
        private List<SelectItem> _availableTags = [];

        public List<SelectItem> AvailableTags
        {
            get => _availableTags;
            set => _availableTags = value;
        }

        public List<SelectItem> Tags
        {
            get => _availableTags;
            set => _availableTags = value;
        }


        // Existing images (edit mode)
        public List<ExistingImageItem> ExistingImages { get; set; } = [];

        // Dropdowns
        public List<SelectItem> Categories { get; set; } = [];
        public List<SelectItem> Brands { get; set; } = [];

        // ── View helpers (computed, not posted) ────────────────
        public bool IsEditMode => Id.HasValue;
        public string PageTitle => IsEditMode ? "Edit Product" : "Create Product";
        public string SubmitLabel => IsEditMode ? "Save Changes" : "Create Product";
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

    public class ExistingImageItem
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public class ExistingVariantItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public decimal? CompareAtPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public bool IsActive { get; set; }
        public List<ExistingAttributeItem> Attributes { get; set; } = [];
    }

    public class ExistingAttributeItem
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

}
