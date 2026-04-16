namespace Payments.Application.DTOs.Checkout
{
    public class VerifyPaymentDto
    {
        public Guid TransactionId { get; set; }
        public string GatewayOrderId { get; set; } = string.Empty;
        public string GatewayPaymentId { get; set; } = string.Empty;
        public string? GatewaySignature { get; set; }
        public Dictionary<string, string> CallbackParams { get; set; } = [];
    }
}