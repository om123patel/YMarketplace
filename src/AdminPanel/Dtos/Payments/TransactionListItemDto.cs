namespace AdminPanel.Dtos.Payments
{
    public class TransactionListItemDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid SellerId { get; set; }
        public decimal Amount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal SellerAmount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? GatewayTransactionId { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}