namespace AdminPanel.ViewModels.Seller
{
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
