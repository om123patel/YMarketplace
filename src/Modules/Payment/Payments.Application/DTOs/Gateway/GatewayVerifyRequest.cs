namespace Payments.Application.DTOs.Gateway
{
    public class GatewayVerifyRequest
    {
        public Guid TransactionId { get; set; }
        public string GatewayOrderId { get; set; } = string.Empty;
        public string GatewayPaymentId { get; set; } = string.Empty;
        public string? GatewaySignature { get; set; }

        /// <summary>Extra params the gateway returns in the callback (key-value).</summary>
        public Dictionary<string, string> CallbackParams { get; set; } = [];
    }
}