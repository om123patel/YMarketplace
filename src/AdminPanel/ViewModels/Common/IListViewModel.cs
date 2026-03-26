using AdminPanel.ViewModels.Grid;

namespace AdminPanel.ViewModels.Common
{
    public interface IListViewModel
    {
        // ── Pagination ───────────────────────────────────────────────────────
        int Page { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }

        // ── Sort ─────────────────────────────────────────────────────────────
        string SortBy { get; }
        string SortDirection { get; }

        // ── Filters ──────────────────────────────────────────────────────────
        TableFilterModel Filters { get; }

        // ── Route data for pagination links ──────────────────────────────────
        // Return all current filter+sort params so pagination preserves state.
        Dictionary<string, string?> RouteData { get; }
    }
}
