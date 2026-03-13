namespace AdminPanel.ViewModels.Products
{
    public class PaginationViewModel
    {
        public int    Page       { get; set; }
        public int    PageSize   { get; set; }
        public int    TotalCount { get; set; }
        public int    TotalPages => PageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        public string Action     { get; set; } = "Index";
        public string Controller { get; set; } = "Home";

        // All current filter/sort values — passed through on page change
        public Dictionary<string, string?> RouteData { get; set; } = [];
    }
}
