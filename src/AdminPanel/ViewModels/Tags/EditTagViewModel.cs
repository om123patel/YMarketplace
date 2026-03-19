using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Tags
{
    public class EditTagViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(80, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug: lowercase letters, numbers, hyphens only")]
        public string Slug { get; set; } = string.Empty;

        public string OriginalName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
