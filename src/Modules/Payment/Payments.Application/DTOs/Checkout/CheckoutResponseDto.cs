namespace Payments.Application.DTOs.Checkout
{
    public class CheckoutResponseDto
    {
        public Guid TransactionId { get; set; }
        public string GatewayOrderId { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;

        /// <summary>Redirect URL for hosted gateways (PayU, Cashfree, Paytm, PhonePe).</summary>
        public string? CheckoutUrl { get; set; }

        /// <summary>SDK options for JS-SDK gateways (Razorpay).</summary>
        public Dictionary<string, string> SdkOptions { get; set; } = [];
    }
}