using AdminPanel.ViewModels.Common;
using AdminPanel.ViewModels.Grid;

namespace AdminPanel.ViewModels.Payments
{
    public class PayoutIndexViewModel
        : PagedListViewModel<PayoutListItemViewModel>
    {
        public string? StatusFilter { get; set; }
        public List<FilterOption> StatusOptions { get; set; } = [];
    }

}
