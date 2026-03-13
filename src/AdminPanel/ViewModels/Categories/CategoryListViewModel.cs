using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Categories
{
    // ── List ─────────────────────────────────────────────────────
    public class CategoryListViewModel
    {
        public List<CategoryItem> Items { get; set; } = [];
        public string? Search { get; set; }
        public string? StatusFilter { get; set; }
        public string SortBy { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";
        public int TotalCount => Items.Count;
        public int ActiveCount => Items.Count(i => i.IsActive);
        public int InactiveCount => Items.Count(i => !i.IsActive);
    }

    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? IconUrl { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int ProductCount { get; set; }
        public int ChildCount { get; set; }
        public int Depth { get; set; }
    }

    // ── Create ───────────────────────────────────────────────────
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug must be lowercase letters, numbers and hyphens")]
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? IconUrl { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; } = 0;
        public List<ParentCategoryOption> ParentOptions { get; set; } = [];
    }

    // ── Edit ─────────────────────────────────────────────────────
    public class EditCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug must be lowercase letters, numbers and hyphens")]
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? IconUrl { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string OriginalName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public List<ParentCategoryOption> ParentOptions { get; set; } = [];
    }

    public class ParentCategoryOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}