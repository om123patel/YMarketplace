namespace Payments.Application.DTOs.Transactions
{
    public class RefundTransactionDto
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}