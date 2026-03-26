namespace AdminPanel.ViewModels.Common
{
    public class EmptyStateModel
    {
        public string Title { get; set; } = "No results found";
        public string? Message { get; set; }
        public string? ActionLabel { get; set; }
        public string? ActionHref { get; set; }
        /// <summary>SVG path data for the icon. Defaults to a box icon.</summary>
        public string? IconPath { get; set; }
    }
}
