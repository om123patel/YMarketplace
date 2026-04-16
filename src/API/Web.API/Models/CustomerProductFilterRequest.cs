namespace Web.API.Models
{
    public class CustomerProductFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string? SortBy { get; set; }
        public string SortDirection { get; set; } = "asc";
    }
}
