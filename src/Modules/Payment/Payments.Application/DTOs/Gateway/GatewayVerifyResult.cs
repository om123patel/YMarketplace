namespace Payments.Application.DTOs.Gateway
{
    public class GatewayVerifyResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? GatewayTransactionId { get; set; }
        public string? GatewayResponse { get; set; }  // raw JSON for audit
        public decimal? AmountVerified { get; set; }  // amount confirmed by gateway
        public string? Status { get; set; }  // gateway-side status string
    }
}