namespace Payments.Application.DTOs.Gateway
{
    public class GatewayRefundResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? GatewayRefundId { get; set; }
        public string? Status { get; set; }
    }
}