namespace Payments.Application.DTOs.Gateway
{
    public class GatewayOrderResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }

        /// <summary>Gateway-assigned order/session ID (store in Transaction.GatewayOrderId).</summary>
        public string? GatewayOrderId { get; set; }

        /// <summary>
        /// For hosted-page gateways (Cashfree, PayU, Paytm) — redirect buyer here.
        /// For JS-SDK gateways (Razorpay, PhonePe) — use GatewayOrderId in frontend SDK.
        /// </summary>
        public string? CheckoutUrl { get; set; }

        /// <summary>Any extra key-value data the frontend needs (SDK options, keys, etc.).</summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }
}