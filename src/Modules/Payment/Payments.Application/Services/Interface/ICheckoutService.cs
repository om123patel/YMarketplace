using Payments.Application.DTOs.Checkout;
using Shared.Application.Models;

namespace Payments.Application.Services.Interface
{
    public interface ICheckoutService
    {
        /// <summary>
        /// Initiates a checkout session for an order.
        /// Creates a Pending Transaction if one doesn't exist, then calls the gateway.
        /// </summary>
        Task<Result<CheckoutResponseDto>> InitiateAsync(
            InitiateCheckoutDto dto,
            Guid buyerId,
            string buyerName,
            string buyerEmail,
            string buyerPhone,
            string orderNumber,
            decimal orderAmount,
            int? categoryId,
            CancellationToken ct = default);

        /// <summary>
        /// Verifies a payment after the buyer completes checkout (callback/return).
        /// Marks the transaction Completed or Failed.
        /// </summary>
        Task<Result<bool>> VerifyAsync(
            VerifyPaymentDto dto,
            CancellationToken ct = default);

        /// <summary>
        /// Processes a gateway webhook event.
        /// </summary>
        Task<Result> HandleWebhookAsync(
            string gatewayName,
            string rawBody,
            IReadOnlyDictionary<string, string> headers,
            CancellationToken ct = default);
    }
}