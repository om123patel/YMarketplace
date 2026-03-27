namespace Identity.Application.DTOs.Seller
{
    public class SellerDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        // 🔹 From User
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }

        // 🔹 Seller Info
        public string BusinessName { get; set; } = string.Empty;
        public string SellerStatus { get; set; } = string.Empty;

        // 🔹 Address (Value Object)
        public string? City { get; set; }
        public string? Country { get; set; }

        // 🔹 Stats
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal? Rating { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
