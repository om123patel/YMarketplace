using Payments.Application.DTOs.Gateway;

namespace Payments.Application.Interfaces
{
    /// <summary>
    /// Abstraction over any payment gateway.
    /// Each gateway implements this interface independently.
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>Unique key matching PaymentGateway enum .ToString()</summary>
        string GatewayName { get; }

        /// <summary>
        /// Create a payment order/session on the gateway and return
        /// the data needed to render a checkout form or redirect.
        /// </summary>
        Task<GatewayOrderResult> CreateOrderAsync(
            GatewayOrderRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Verify a payment after the buyer completes checkout.
        /// Returns success + gateway transaction ID if valid.
        /// </summary>
        Task<GatewayVerifyResult> VerifyPaymentAsync(
            GatewayVerifyRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Process a refund via the gateway.
        /// </summary>
        Task<GatewayRefundResult> RefundAsync(
            GatewayRefundRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Verify and parse an incoming webhook payload.
        /// Returns null if signature validation fails.
        /// </summary>
        GatewayWebhookResult? ParseWebhook(
            string rawBody,
            IReadOnlyDictionary<string, string> headers);
    }
}