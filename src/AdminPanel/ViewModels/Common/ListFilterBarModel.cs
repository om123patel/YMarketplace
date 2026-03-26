namespace AdminPanel.ViewModels.Common
{
    public class ListFilterBarModel
    {
        public string FormId { get; set; } = "filterForm";
        public string? SearchValue { get; set; }
        public string SearchParam { get; set; } = "search";
        public string Placeholder { get; set; } = "Search…";
        public bool HasFilters { get; set; }
        public int FilterCount { get; set; }
        public string ClearUrl { get; set; } = "";

        /// <summary>Quick-select status tabs rendered in the filter bar.</summary>
        public List<ListStatusTab> StatusTabs { get; set; } = [];
    }

    public class ListStatusTab
    {
        public string Label { get; set; } = string.Empty;
        public string Href { get; set; } = "#";
        public bool IsActive { get; set; }
        public int? Count { get; set; }

        // ── Factory methods for each page ────────────────────────────────────
        // Centralised here so Index.cshtml never builds tab hrefs manually.

        public static List<ListStatusTab> ProductTabs(
            string? currentStatus,
            string baseUrl = "/products")
        {
            string Tab(string? s) => string.IsNullOrEmpty(s)
                ? baseUrl
                : $"{baseUrl}?status={s}";

            return
            [
                new() { Label = "All",      Href = Tab(null),              IsActive = string.IsNullOrEmpty(currentStatus) },
                new() { Label = "Pending",  Href = Tab("PendingApproval"), IsActive = currentStatus == "PendingApproval" },
                new() { Label = "Active",   Href = Tab("Active"),          IsActive = currentStatus == "Active" },
                new() { Label = "Draft",    Href = Tab("Draft"),           IsActive = currentStatus == "Draft" },
                new() { Label = "Rejected", Href = Tab("Rejected"),        IsActive = currentStatus == "Rejected" }
            ];
        }

        public static List<ListStatusTab> UserTabs(
            string? currentStatus,
            string baseUrl = "/Users")
        {
            string Tab(string? s) => string.IsNullOrEmpty(s)
                ? baseUrl
                : $"{baseUrl}?status={s}";

            return
            [
                new() { Label = "All",       Href = Tab(null),                   IsActive = string.IsNullOrEmpty(currentStatus) },
                new() { Label = "Active",    Href = Tab("Active"),               IsActive = currentStatus == "Active" },
                new() { Label = "Suspended", Href = Tab("Suspended"),            IsActive = currentStatus == "Suspended" },
                new() { Label = "Pending",   Href = Tab("PendingVerification"),  IsActive = currentStatus == "PendingVerification" }
            ];
        }

        public static List<ListStatusTab> SellerTabs(
            string? currentStatus,
            string baseUrl = "/Sellers")
        {
            string Tab(string? s) => string.IsNullOrEmpty(s)
                ? baseUrl
                : $"{baseUrl}?sellerStatus={s}";

            return
            [
                new() { Label = "All",       Href = Tab(null),               IsActive = string.IsNullOrEmpty(currentStatus) },
                new() { Label = "Pending",   Href = Tab("PendingApproval"), IsActive = currentStatus == "PendingApproval" },
                new() { Label = "Active",    Href = Tab("Active"),           IsActive = currentStatus == "Active" },
                new() { Label = "Suspended", Href = Tab("Suspended"),        IsActive = currentStatus == "Suspended" }
            ];
        }
    }
}
