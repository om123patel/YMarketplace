namespace AdminPanel.ViewModels.Tags
{
    public class TagItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
