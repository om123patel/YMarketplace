namespace Payments.Application.DTOs.Payouts
{
    public class PayoutListItemDto
    {
        public Guid Id { get; set; }
        public Guid SellerId { get; set; }
        public string? SellerName { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? UpiId { get; set; }
        public string? BankAccountNumber { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}