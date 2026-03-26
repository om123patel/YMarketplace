// ─────────────────────────────────────────────────────────────────────────────
// Helpers/BadgeHelper.cs
//
// Single source of truth for badge CSS classes and display labels.
// Every Index view that shows a status badge calls these static methods
// instead of duplicating switch expressions.
//
// USAGE in .cshtml:
//   @using AdminPanel.Helpers
//   <span class="@BadgeHelper.ProductStatus(p.Status)">
//       @BadgeHelper.ProductStatusLabel(p.Status)
//   </span>
// ─────────────────────────────────────────────────────────────────────────────

namespace AdminPanel.Helpers
{
    public static class BadgeHelper
    {
        // ── Product status ───────────────────────────────────────────────────
        public static string ProductStatus(string status) => status switch
        {
            "Active" => "badge badge-emerald",
            "PendingApproval" => "badge badge-amber",
            "Rejected" => "badge badge-rose",
            "Archived" => "badge badge-gray",
            "Draft" => "badge badge-sky",
            _ => "badge badge-gray"
        };

        public static string ProductStatusLabel(string status) => status switch
        {
            "PendingApproval" => "Pending",
            _ => status
        };

        // ── User / account status ────────────────────────────────────────────
        public static string UserStatus(string status) => status switch
        {
            "Active" => "badge badge-emerald",
            "Suspended" => "badge badge-rose",
            "Inactive" => "badge badge-gray",
            "PendingVerification" => "badge badge-amber",
            _ => "badge badge-gray"
        };

        public static string UserStatusLabel(string status) => status switch
        {
            "PendingVerification" => "Pending",
            _ => status
        };

        // ── Seller status ────────────────────────────────────────────────────
        public static string SellerStatus(string status) => status switch
        {
            "Active" => "badge badge-emerald",
            "PendingApproval" => "badge badge-amber",
            "Suspended" => "badge badge-rose",
            "Rejected" => "badge badge-gray",
            _ => "badge badge-gray"
        };

        public static string SellerStatusLabel(string status) => status switch
        {
            "PendingApproval" => "Pending",
            _ => status
        };

        // ── User role ────────────────────────────────────────────────────────
        public static string UserRole(string role) => role switch
        {
            "Admin" => "badge badge-indigo",
            "Seller" => "badge badge-sky",
            "Buyer" => "badge badge-gray",
            _ => "badge badge-gray"
        };

        // ── Boolean / generic ────────────────────────────────────────────────
        public static string Active(bool isActive) =>
            isActive ? "badge badge-emerald" : "badge badge-gray";

        public static string ActiveLabel(bool isActive) =>
            isActive ? "Active" : "Inactive";

        // ── Currency symbol ──────────────────────────────────────────────────
        public static string CurrencySymbol(string code) => code switch
        {
            "INR" => "₹",
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            _ => code
        };
    }
}
