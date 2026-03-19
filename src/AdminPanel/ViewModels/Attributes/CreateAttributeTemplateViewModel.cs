using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Attributes
{
    public class CreateAttributeTemplateViewModel
    {
        [Required(ErrorMessage = "Template name is required")]
        [StringLength(150, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        public List<AttributeItemFormRow> Items { get; set; } = [];
        public List<CategoryOption> Categories { get; set; } = [];
    }
}
