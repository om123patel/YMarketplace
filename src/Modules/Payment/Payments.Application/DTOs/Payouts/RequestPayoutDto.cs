namespace Payments.Application.DTOs.Payouts
{
    public class RequestPayoutDto
    {
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public string? BankAccountNumber { get; set; }
        public string? BankIfscCode { get; set; }
        public string? BankAccountName { get; set; }
        public string? UpiId { get; set; }
    }
}