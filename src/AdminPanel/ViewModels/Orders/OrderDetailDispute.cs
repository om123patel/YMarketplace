namespace AdminPanel.ViewModels.Orders
{
    public class OrderDetailDispute
    {
        public Guid Id { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? BuyerEvidence { get; set; }
        public string? SellerResponse { get; set; }
        public string? AdminNote { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }
        public string? Resolution { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
