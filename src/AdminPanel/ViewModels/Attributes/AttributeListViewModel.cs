using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Attributes
{

    public class AttributeListViewModel
    {
        public List<AttributeTemplateListItem> Items { get; set; } = [];
        public string? Search { get; set; }
        public string? StatusFilter { get; set; }
        public string SortBy { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";
        public int TotalCount => Items.Count;
        public int ActiveCount => Items.Count(i => i.IsActive);
        public int InactiveCount => Items.Count(i => !i.IsActive);
    }

    public class AttributeTemplateListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

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

    public class AttributeItemFormRow
    {
        [Required(ErrorMessage = "Attribute name is required")]
        public string AttributeName { get; set; } = string.Empty;
        public string InputType { get; set; } = "Text";
        public string? OptionsRaw { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }

    public class CategoryOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AttributeRowViewModel
    {
        public AttributeItemFormRow Row { get; set; } = new();
        public int Index { get; set; }
    }

}
