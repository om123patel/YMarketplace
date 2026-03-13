namespace AdminPanel.Dtos.Attributes
{
    public class UpdateAttributeTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<AttributeItemRequest> Items { get; set; } = [];
    }
}
