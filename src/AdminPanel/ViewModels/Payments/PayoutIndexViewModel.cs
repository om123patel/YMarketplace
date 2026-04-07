// src/AdminPanel/ViewModels/Payments/PayoutIndexViewModel.cs
using AdminPanel.ViewModels.Common;
using AdminPanel.ViewModels.Grid;

namespace AdminPanel.ViewModels.Payments
{
    public class PayoutIndexViewModel
        : PagedListViewModel<PayoutListItemViewModel>
    {
        public List<FilterOption> StatusOptions { get; set; } = [];

        // Route data for pagination
        public Dictionary<string, string?> RouteValues { get; set; } = [];
    }
}