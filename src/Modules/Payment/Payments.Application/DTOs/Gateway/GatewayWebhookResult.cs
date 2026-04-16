namespace Payments.Application.DTOs.Gateway
{
    public class GatewayWebhookResult
    {
        public string EventType { get; set; } = string.Empty;   // "payment.captured", "payment.failed", etc.
        public string GatewayOrderId { get; set; } = string.Empty;
        public string GatewayPaymentId { get; set; } = string.Empty;
        public string? GatewaySignature { get; set; }
        public decimal? Amount { get; set; }
        public string? Status { get; set; }
        public bool IsSuccess => Status is "captured" or "completed" or "success" or "CHARGED";
        public bool IsFailure => Status is "failed" or "FAILED" or "FAILED_PAYMENT";
        public string? RawBody { get; set; }
    }
}