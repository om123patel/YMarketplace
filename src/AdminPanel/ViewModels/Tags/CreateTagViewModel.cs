using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Tags
{
    public class CreateTagViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(80, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug: lowercase letters, numbers, hyphens only")]
        public string Slug { get; set; } = string.Empty;
    }
}
