namespace AdminPanel.Dtos.Brands
{
    public class CreateBrandRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Website { get; set; }
    }
}
