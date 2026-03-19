using AdminPanel.ViewModels.Common;

namespace AdminPanel.ViewModels.Auth
{
    public class UserListViewModel : PagedListViewModel<UserListItem>
    {
        public string? Role { get; set; }

        public static readonly string[] RoleOptions = ["Admin", "Seller", "Buyer"];
        public static readonly string[] StatusOptions =
            ["Active", "Inactive", "Suspended", "PendingVerification"];
    }

}
