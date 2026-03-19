using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Brands
{
    public class CreateBrandViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug: lowercase letters, numbers, hyphens only")]
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? WebsiteUrl { get; set; }
    }
}
