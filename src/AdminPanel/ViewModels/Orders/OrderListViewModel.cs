using AdminPanel.ViewModels.Common;
using AdminPanel.ViewModels.Grid;

namespace AdminPanel.ViewModels.Orders
{
    public class OrderListViewModel : IListViewModel
    {
        public List<OrderListItem> Items { get; set; } = [];
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public string SortBy { get; set; } = "createdat";
        public string SortDirection { get; set; } = "desc";
        public TableFilterModel Filters { get; set; } = new();
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }

        public Dictionary<string, string?> RouteData => new()
        {
            ["search"] = Search,
            ["status"] = Status,
            ["paymentStatus"] = PaymentStatus,
            ["sortBy"] = SortBy,
            ["sortDirection"] = SortDirection
        };
    }

    
}