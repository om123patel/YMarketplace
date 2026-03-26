namespace AdminPanel.ViewModels.Grid
{
    public class FilterOption
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        /// <summary>
        /// Optional CSS class for a colour dot (e.g. "dot-emerald", "dot-amber").
        /// Rendered as <span class="filter-dot {Color}"> next to the label.
        /// </summary>
        public string? Color { get; set; }

        public FilterOption() { }
        public FilterOption(string value, string label, string? color = null)
        {
            Value = value; Label = label; Color = color;
        }
    }
}
