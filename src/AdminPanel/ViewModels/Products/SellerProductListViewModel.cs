using AdminPanel.ViewModels.Common;
using AdminPanel.ViewModels.Grid;

namespace AdminPanel.ViewModels.Products
{
    public class SellerProductListViewModel : PagedListViewModel<SellerProductListItem>
    {
        public int? CategoryId { get; set; }
        public List<FilterOption> Categories { get; set; } = [];
    }

  

}
