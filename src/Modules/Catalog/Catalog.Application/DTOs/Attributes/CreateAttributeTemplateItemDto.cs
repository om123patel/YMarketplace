namespace Catalog.Application.DTOs.Attributes
{
    public class CreateAttributeTemplateItemDto
    {
        public string AttributeName { get; set; } = string.Empty;
        public string InputType { get; set; } = "Text";
        // Text | Select | MultiSelect | Number | Boolean
        public List<string> Options { get; set; } = [];
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }

}
