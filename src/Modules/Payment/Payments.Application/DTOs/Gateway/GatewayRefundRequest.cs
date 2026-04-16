namespace Payments.Application.DTOs.Gateway
{
    public class GatewayRefundRequest
    {
        public string GatewayPaymentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid TransactionId { get; set; }
    }
}