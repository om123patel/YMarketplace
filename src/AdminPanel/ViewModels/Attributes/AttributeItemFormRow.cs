using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Attributes
{
    public class AttributeItemFormRow
    {
        [Required(ErrorMessage = "Attribute name is required")]
        public string AttributeName { get; set; } = string.Empty;
        public string InputType { get; set; } = "Text";
        public string? OptionsRaw { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }
}
