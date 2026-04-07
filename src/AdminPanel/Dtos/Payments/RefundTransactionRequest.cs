namespace AdminPanel.Dtos.Payments
{
    public class RefundTransactionRequest
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}