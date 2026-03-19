namespace AdminPanel.ViewModels.Categories
{
    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? IconUrl { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int ProductCount { get; set; }
        public int ChildCount { get; set; }
        public int Depth { get; set; }
    }
}
