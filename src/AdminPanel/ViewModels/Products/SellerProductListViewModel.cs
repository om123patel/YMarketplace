using AdminPanel.ViewModels.Common;

namespace AdminPanel.ViewModels.Products
{
    public class SellerProductListViewModel : PagedListViewModel<SellerProductListItem>
    {
        public int? CategoryId { get; set; }
        public List<FilterOption> Categories { get; set; } = [];
    }

  

}
