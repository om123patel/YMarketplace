using AdminPanel.ViewModels.Common;

namespace AdminPanel.ViewModels.Products
{
    public class ProductListViewModel : PagedListViewModel<ProductListItem>
    {
        // Product-specific filters
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string? CreatorType { get; set; }

        // Filter dropdowns
        public List<FilterOption> Categories { get; set; } = [];
        public List<FilterOption> Brands { get; set; } = [];

        // Status options for dropdown
        public static readonly string[] StatusOptions =
            ["Active", "PendingApproval", "Draft", "Rejected", "Archived"];
    }
}
