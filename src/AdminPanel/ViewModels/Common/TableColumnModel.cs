namespace AdminPanel.ViewModels.Common
{
    public class TableColumnModel
    {
        // ── Display ──────────────────────────────────────────────────────────
        public string Label { get; set; } = string.Empty;
        public string? Width { get; set; }          // e.g. "120px", "10%"
        public string Align { get; set; } = "left"; // left | center | right

        public string CssClass
        {
            get
            {
                var classes = new List<string>();
                if (Sortable) classes.Add("sortable");
                if (IsActiveSort) classes.Add("active-sort");
                return string.Join(" ", classes);
            }
        }

        // ── Sort ─────────────────────────────────────────────────────────────
        public bool Sortable { get; set; }
        public string Column { get; set; } = string.Empty; // API sort key
        public string SortBy { get; set; } = string.Empty; // current sort
        public string SortDirection { get; set; } = "asc";        // current dir

        // ── Request (needed to build the sort href) ───────────────────────────
        /// <summary>
        /// Pass HttpContext.Request so the column builds its own href by
        /// copying all existing query params and flipping just sortBy/sortDirection.
        /// Avoids the giant SortHref() dictionary in every Index.cshtml.
        /// </summary>
        public HttpRequest? Request { get; set; }

        // ── Computed ─────────────────────────────────────────────────────────
        public bool IsActiveSort =>
            Sortable && string.Equals(Column, SortBy, StringComparison.OrdinalIgnoreCase);

        public string NextDirection =>
            IsActiveSort && SortDirection == "asc" ? "desc" : "asc";

        public string SortIcon =>
            !IsActiveSort ? "↕" : SortDirection == "asc" ? "↑" : "↓";

        /// <summary>
        /// Builds the sort href from the current request query string,
        /// overwriting only sortBy, sortDirection, and page=1.
        /// </summary>
        public string SortHref
        {
            get
            {
                if (Request is null) return "#";

                var qs = System.Web.HttpUtility.ParseQueryString(
                    Request.QueryString.ToString());

                qs["sortBy"] = Column;
                qs["sortDirection"] = NextDirection;
                qs["page"] = "1";

                return Request.Path + "?" + qs.ToString();
            }
        }
    }
}
