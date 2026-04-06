using AdminPanel.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AdminPanel.TagHelpers
{
    [HtmlTargetElement("table-column")]
    public sealed class TableColumnTagHelper : TagHelper
    {
        private readonly IHtmlHelper _htmlHelper;

        public TableColumnTagHelper(IHtmlHelper htmlHelper) => _htmlHelper = htmlHelper;

        [HtmlAttributeName("label")]
        public string? Label { get; set; }

        [HtmlAttributeName("column")]
        public string? Column { get; set; }

        [HtmlAttributeName("width")]
        public string? Width { get; set; }

        [HtmlAttributeName("align")]
        public string Align { get; set; } = "left";

        [HtmlAttributeName("sortable")]
        public bool Sortable { get; set; } = true;

        [HtmlAttributeName("sort-by")]
        public string? SortBy { get; set; }

        [HtmlAttributeName("sort-direction")]
        public string? SortDirection { get; set; }

        [ViewContext, HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Build model the same way your inline function did
            var model = new TableColumnModel
            {
                Label = Label,
                Sortable = Sortable,
                Column = Column,
                SortBy = SortBy,
                SortDirection = SortDirection,
                Request = ViewContext?.HttpContext.Request,
                Width = Width,
                Align = Align
            };

            // Contextualize the HtmlHelper so it can render the partial
            ((IViewContextAware)_htmlHelper).Contextualize(ViewContext!);

            // Render the existing partial to keep markup consistent
            var content = await _htmlHelper.PartialAsync("_TableColumn", model);

            output.TagName = null; // we output the partial's markup only
            output.Content.SetHtmlContent(content);
        }
    }
}