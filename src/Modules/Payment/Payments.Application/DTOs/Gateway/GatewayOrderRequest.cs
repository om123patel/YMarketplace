namespace Payments.Application.DTOs.Gateway
{
    public class GatewayOrderRequest
    {
        /// <summary>Our internal transaction ID — used as receipt/reference.</summary>
        public Guid TransactionId { get; set; }

        /// <summary>Our internal order number.</summary>
        public string OrderNumber { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "INR";

        // Buyer info
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerEmail { get; set; } = string.Empty;
        public string BuyerPhone { get; set; } = string.Empty;

        // Return/callback URLs
        public string SuccessUrl { get; set; } = string.Empty;
        public string FailureUrl { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;

        /// <summary>Optional: pre-selected payment method hint.</summary>
        public string? PreferredMethod { get; set; }
    }
}