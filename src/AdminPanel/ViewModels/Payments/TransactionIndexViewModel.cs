using AdminPanel.ViewModels.Common;
using AdminPanel.ViewModels.Grid;

namespace AdminPanel.ViewModels.Payments
{
    public class TransactionIndexViewModel
        : PagedListViewModel<TransactionListItemViewModel>
    {
        public string? StatusFilter { get; set; }
        public string? MethodFilter { get; set; }
        public string? DateFromFilter { get; set; }
        public string? DateToFilter { get; set; }

        public List<FilterOption> StatusOptions { get; set; } = [];
        public List<FilterOption> MethodOptions { get; set; } = [];
    }

}
