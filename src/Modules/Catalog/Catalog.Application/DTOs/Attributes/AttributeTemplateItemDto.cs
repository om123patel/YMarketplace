namespace Catalog.Application.DTOs.Attributes
{
    public class AttributeTemplateItemDto
    {
        public int Id { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public string? Options { get; set; }         // raw JSON string
        public List<string> ParsedOptions { get; set; } = [];  // parsed for UI
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }

}
