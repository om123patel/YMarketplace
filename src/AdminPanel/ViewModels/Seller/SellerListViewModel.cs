using AdminPanel.ViewModels.Common;

namespace AdminPanel.ViewModels.Seller
{
    public class SellerListViewModel : PagedListViewModel<SellerListItem>
    {
        public string? SellerStatus { get; set; }  // separate from base StatusFilter
        public int PendingCount { get; set; }

        public static readonly string[] StatusOptions =
            ["PendingApproval", "Active", "Rejected", "Suspended"];
    }
}
