namespace Catalog.Application.DTOs.Attributes
{
    public class UpdateAttributeTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<CreateAttributeTemplateItemDto> Items { get; set; } = [];
    }

}
