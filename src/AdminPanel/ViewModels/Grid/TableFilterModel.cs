namespace AdminPanel.ViewModels.Grid
{
    public class TableFilterModel
    {
        /// <summary>Must match the id= on the wrapping &lt;form&gt;.</summary>
        public string FormId { get; set; } = "filterForm";

        public List<ColumnFilterDef> Columns { get; set; } = [];

        /// <summary>URL to clear all filters (e.g. "/products").</summary>
        public string ClearAllUrl { get; set; } = "";

        /// <summary>Open the panel on page load even with no active filters.</summary>
        public bool StartOpen { get; set; } = false;

        // Computed
        public int ActiveCount => Columns.Count(c => c.IsActive);
        public bool HasActiveFilters => ActiveCount > 0;
    }
}
