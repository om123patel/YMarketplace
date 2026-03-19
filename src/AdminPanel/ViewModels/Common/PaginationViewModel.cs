namespace AdminPanel.ViewModels.Common
{

    public class PaginationViewModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public string Action { get; set; } = "Index";
        public string Controller { get; set; } = string.Empty;
        public Dictionary<string, string?> RouteData { get; set; } = [];

        public int TotalPages => PageSize > 0
                                       ? (int)Math.Ceiling((double)TotalCount / PageSize)
                                       : 0;
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public int FirstItemIndex => TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int LastItemIndex => Math.Min(Page * PageSize, TotalCount);
    }

}
