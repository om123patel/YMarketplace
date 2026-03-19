using AdminPanel.ViewModels.Common;
using System.ComponentModel.DataAnnotations;

namespace AdminPanel.ViewModels.Attributes
{

    public class AttributeListViewModel : PagedListViewModel<AttributeTemplateListItem>
    {
        // no extra filters beyond base Search + StatusFilter
    }


}
