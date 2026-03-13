namespace AdminPanel.ViewModels.Auth
{
    public class UserListViewModel
    {
        public List<UserListItem> Items { get; set; } = [];

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        // Filters
        public string? Search { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public string SortBy { get; set; } = "createdat";
        public string SortDirection { get; set; } = "desc";
    }

    public class UserListItem
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserDetailViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Action flags — computed by controller
        public bool CanSuspend { get; set; }
        public bool CanActivate { get; set; }
        public bool CanDelete { get; set; }
    }

    public class SellerListViewModel
    {
        public List<SellerListItem> Items { get; set; } = [];

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        public string? Search { get; set; }
        public string? SellerStatus { get; set; }
        public string SortBy { get; set; } = "createdat";
        public string SortDirection { get; set; } = "desc";

        // Quick count for the pending badge in the sidebar
        public int PendingCount { get; set; }
    }

    public class SellerListItem
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Country { get; set; }
        public string SellerStatus { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal? Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    // SELLER DETAIL
    // ══════════════════════════════════════════════════════════
    public class SellerDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string UserStatus { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string? BusinessEmail { get; set; }
        public string? BusinessPhone { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string SellerStatus { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal? Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Action flags
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanSuspend { get; set; }
        public bool CanActivate { get; set; }
    }


}
