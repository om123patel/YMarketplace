namespace AdminPanel.ViewModels.Payments
{
    public class PayoutDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid SellerId { get; set; }
        public string? SellerName { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? BankAccountNumber { get; set; }
        public string? BankIfscCode { get; set; }
        public string? BankAccountName { get; set; }
        public string? UpiId { get; set; }
        public string? AdminNote { get; set; }
        public string? FailureReason { get; set; }
        public string? GatewayReference { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Action form fields
        public string? GatewayReferenceInput { get; set; }
        public string? AdminNoteInput { get; set; }
        public string? FailureReasonInput { get; set; }
        public string? CancelReasonInput { get; set; }

        public bool CanProcess => Status == "Pending";
        public bool CanComplete => Status == "Processing";
        public bool CanFail => Status == "Processing";
        public bool CanCancel =>
            Status is "Pending" or "Processing";
    }

}
