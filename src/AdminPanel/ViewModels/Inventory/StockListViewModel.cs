using AdminPanel.Dtos.Inventory;
using AdminPanel.ViewModels.Common;
using AdminPanel.ViewModels.Grid;

namespace AdminPanel.ViewModels.Inventory
{
    public class StockListViewModel : IListViewModel
    {
        public List<StockListItem> Items { get; set; } = [];
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public string SortBy { get; set; } = "updatedat";
        public string SortDirection { get; set; } = "desc";
        public TableFilterModel Filters { get; set; } = new();

        public string? Search { get; set; }
        public string? Status { get; set; }
        public bool? LowStockOnly { get; set; }

        public int LowStockCount { get; set; }

        public Dictionary<string, string?> RouteData => new()
        {
            ["search"] = Search,
            ["status"] = Status,
            ["lowStockOnly"] = LowStockOnly?.ToString().ToLower(),
            ["sortBy"] = SortBy,
            ["sortDirection"] = SortDirection
        };
    }

   
}