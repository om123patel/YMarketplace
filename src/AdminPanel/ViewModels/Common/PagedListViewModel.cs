namespace AdminPanel.ViewModels.Common
{
    public abstract class PagedListViewModel<TItem>
    {
        public List<TItem> Items { get; set; } = [];
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0
                                        ? (int)Math.Ceiling((double)TotalCount / PageSize)
                                        : 0;
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public int FirstItemIndex => TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int LastItemIndex => Math.Min(Page * PageSize, TotalCount);

        // Sorting
        public string SortBy { get; set; } = "createdat";
        public string SortDirection { get; set; } = "desc";

        // Search & status filter — shared by all lists
        public string? Search { get; set; }
        public string? StatusFilter { get; set; }

        // Route data for _Pagination partial
        public Dictionary<string, string?> RouteData { get; private set; } = [];

        // Call this after setting all filters so pagination links carry them
        public void BuildRouteData(Dictionary<string, string?>? extra = null)
        {
            RouteData = new Dictionary<string, string?>
            {
                ["search"] = Search,
                ["status"] = StatusFilter,
                ["sortBy"] = SortBy,
                ["sortDirection"] = SortDirection
            };
            if (extra is null) return;
            foreach (var kv in extra)
                RouteData[kv.Key] = kv.Value;
        }

        // View helpers
        public bool IsEmpty => !Items.Any();
        public bool HasActiveFilter => !string.IsNullOrWhiteSpace(Search)
                                      || !string.IsNullOrWhiteSpace(StatusFilter);
        public string NextDirection(string col)
            => SortBy == col && SortDirection == "asc" ? "desc" : "asc";
        public string SortIcon(string col)
            => SortBy != col ? "↕" : SortDirection == "asc" ? "↑" : "↓";
    }

    
}