namespace Orders.Application.DTOs.Dispute
{
    public class DisputeDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid BuyerId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? BuyerEvidence { get; set; }
        public string? SellerResponse { get; set; }
        public string? AdminNote { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? ResolvedByAdminId { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? Resolution { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
