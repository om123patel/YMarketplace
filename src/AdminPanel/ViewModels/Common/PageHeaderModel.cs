namespace AdminPanel.ViewModels.Common
{
    public class PageHeaderModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }

        /// <summary>
        /// Raw HTML for action buttons in the top-right corner.
        /// Use Html.Raw(...) when rendering.
        /// </summary>
        public string? ActionsHtml { get; set; }
    }

    /// <summary>
    /// Optional alert shown below the page header (maps to TempData keys).
    /// </summary>
    public class AlertModel
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "success"; // success | error | warning | info
    }
}
