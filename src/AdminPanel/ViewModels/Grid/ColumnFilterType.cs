namespace AdminPanel.ViewModels.Grid
{
    public enum ColumnFilterType
    {
        Text,             // free-text input
        Select,           // single <select>
        Checkbox,         // multi-checkbox (values joined with comma)
        Radio,            // radio button group
        Date,             // single date picker
        DateRange,        // from → to date pickers
        NumberComparison  // operator (=, ≥, ≤, between) + value(s)
    }
}
