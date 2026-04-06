namespace AdminPanel.ViewModels.Payments
{
    public class PayoutListItemViewModel
    {
        public Guid Id { get; set; }
        public Guid SellerId { get; set; }
        public string? SellerName { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }  // "UPI: xxx" or "Bank: xxx"
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
