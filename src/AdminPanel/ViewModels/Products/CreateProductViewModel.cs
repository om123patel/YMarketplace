using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Products
{
    public class CreateProductViewModel
    {
        // ── Mode ───────────────────────────────────────────────
        public Guid? Id { get; set; }
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

        // ── Step 5: Organisation ───────────────────────────────
        public bool IsFeatured { get; set; }
        public string? Status { get; set; }
        public string? RejectionReason { get; set; }

        // ── Tags ───────────────────────────────────────────────
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

        // ── Images (edit mode) ─────────────────────────────────
        public List<ExistingImageItem> ExistingImages { get; set; } = [];

        // ── Dropdowns ──────────────────────────────────────────
        public List<SelectItem> Categories { get; set; } = [];
        public List<SelectItem> Brands { get; set; } = [];

        // ── Computed helpers ───────────────────────────────────
        public bool IsEditMode => Id.HasValue;
        public string PageTitle => IsEditMode ? "Edit Product" : "Create Product";
        public string SubmitLabel => IsEditMode ? "Save Changes" : "Create Product";
    }

    public class SelectItem
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
