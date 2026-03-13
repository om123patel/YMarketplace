namespace AdminPanel.Dtos.Attributes
{
    

    public class AttributeTemplateItemDto
    {
        public int Id { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public string? Options { get; set; }
        public List<string> ParsedOptions { get; set; } = [];
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }
}
