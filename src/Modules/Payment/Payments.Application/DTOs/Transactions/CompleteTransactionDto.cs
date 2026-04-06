namespace Payments.Application.DTOs.Transactions
{
    public class CompleteTransactionDto
    {
        public string GatewayTransactionId { get; set; } = string.Empty;
        public string? GatewayResponse { get; set; }
    }
}