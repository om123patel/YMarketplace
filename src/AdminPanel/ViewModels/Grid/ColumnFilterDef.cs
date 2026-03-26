using AdminPanel.ViewModels.Common;

namespace AdminPanel.ViewModels.Grid
{
    public class ColumnFilterDef
    {
        // Shared
        public string Label { get; set; } = string.Empty;
        public string ParamName { get; set; } = string.Empty;
        public string? CurrentValue { get; set; }
        public ColumnFilterType Type { get; set; } = ColumnFilterType.Text;
        public string? Placeholder { get; set; }
        public List<FilterOption> Options { get; set; } = [];

        // DateRange — second param/value
        public string? DateToParamName { get; set; }
        public string? CurrentValueTo { get; set; }

        // Checkbox — comma-separated checked values (computed from CurrentValue)
        public IEnumerable<string> CheckedValues =>
            string.IsNullOrEmpty(CurrentValue)
                ? []
                : CurrentValue.Split(',', StringSplitOptions.RemoveEmptyEntries
                                        | StringSplitOptions.TrimEntries);

        // NumberComparison
        public string? OpParamName { get; set; }  // e.g. "priceOp"
        public string CurrentOp { get; set; } = "gte"; // gte | lte | eq | between
        public string? NumberToParamName { get; set; }  // e.g. "priceMax"
        public string? CurrentValueMax { get; set; }
        public string? Unit { get; set; }  // e.g. "₹"

        // Computed: is this filter currently active?
        public bool IsActive =>
            !string.IsNullOrEmpty(CurrentValue) ||
            !string.IsNullOrEmpty(CurrentValueTo) ||
            !string.IsNullOrEmpty(CurrentValueMax);
    }
}
