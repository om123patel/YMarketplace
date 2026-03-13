namespace AdminPanel.Dtos.Attributes
{
   

   
    public class AttributeItemRequest
    {
        public string AttributeName { get; set; } = string.Empty;
        public string InputType { get; set; } = "Text";
        public List<string> Options { get; set; } = [];
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }
}
