namespace AdminPanel.ViewModels.Payments
{
    public class TransactionDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid BuyerId { get; set; }
        public Guid SellerId { get; set; }
        public Guid StoreId { get; set; }
        public decimal Amount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal SellerAmount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? GatewayTransactionId { get; set; }
        public string? GatewayProvider { get; set; }
        public decimal RefundedAmount { get; set; }
        public string? RefundReason { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Refund form
        public decimal RefundAmount { get; set; }
        public string? RefundReasonInput { get; set; }
        public decimal MaxRefundable => Amount - RefundedAmount;
        public bool CanRefund =>
            Status is "Completed" or "PartiallyRefunded"
            && MaxRefundable > 0;
    }

}
