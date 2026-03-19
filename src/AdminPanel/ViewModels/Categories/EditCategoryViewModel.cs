using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Categories
{
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
}
