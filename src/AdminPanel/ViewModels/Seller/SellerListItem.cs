namespace AdminPanel.ViewModels.Seller
{
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
}
