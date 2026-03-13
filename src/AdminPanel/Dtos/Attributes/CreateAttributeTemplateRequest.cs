namespace AdminPanel.Dtos.Attributes
{
    public class CreateAttributeTemplateRequest
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<AttributeItemRequest> Items { get; set; } = [];
    }
}
