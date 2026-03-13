namespace Catalog.Application.DTOs
{
    public class CreateAttributeTemplateDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<CreateAttributeTemplateItemDto> Items { get; set; } = [];
    }
}
