using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Attributes
{
    public class EditAttributeTemplateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Template name is required")]
        [StringLength(150, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        public bool IsActive { get; set; }

        public List<AttributeItemFormRow> Items { get; set; } = [];
        public List<CategoryOption> Categories { get; set; } = [];

        public string OriginalName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
