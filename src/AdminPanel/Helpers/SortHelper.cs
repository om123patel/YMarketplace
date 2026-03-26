namespace AdminPanel.Helpers
{
    public static class SortHelper
    {
        /// <summary>
        /// Builds a URL that preserves ALL current query params,
        /// only flipping sortBy / sortDirection / page.
        /// Adding new filters to a view never requires touching this.
        /// </summary>
        public static string SortUrl(
            HttpRequest request,
            string column,
            string currentSortBy,
            string currentSortDirection)
        {
            var newDir = currentSortBy.Equals(column, StringComparison.OrdinalIgnoreCase)
                         && currentSortDirection == "asc"
                         ? "desc" : "asc";

            var qs = request.Query
                .Where(q =>
                    !q.Key.Equals("sortBy", StringComparison.OrdinalIgnoreCase) &&
                    !q.Key.Equals("sortDirection", StringComparison.OrdinalIgnoreCase) &&
                    !q.Key.Equals("page", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(q => q.Key, q => q.Value.ToString());

            qs["sortBy"] = column;
            qs["sortDirection"] = newDir;
            qs["page"] = "1";

            return "?" + string.Join("&",
                qs.Select(k => $"{k.Key}={Uri.EscapeDataString(k.Value)}"));
        }

        /// <summary>Returns "asc", "desc", or "none" — used as CSS state class.</summary>
        public static string SortState(
            string column,
            string currentSortBy,
            string currentSortDirection)
        {
            if (!column.Equals(currentSortBy, StringComparison.OrdinalIgnoreCase))
                return "none";

            return currentSortDirection == "asc" ? "asc" : "desc";
        }
    }
}
