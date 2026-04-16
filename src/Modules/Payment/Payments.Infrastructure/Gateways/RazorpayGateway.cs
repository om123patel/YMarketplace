using Microsoft.Extensions.Options;
using Payments.Application.DTOs.Gateway;
using Payments.Application.Interfaces;
using Payments.Infrastructure.Gateways.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Payments.Infrastructure.Gateways
{
    /// <summary>
    /// Razorpay Payment Gateway.
    /// Docs: https://razorpay.com/docs/
    /// Flow: Create Order via API → Frontend uses Razorpay JS SDK → Verify signature on callback.
    /// </summary>
    public class RazorpayGateway : IPaymentGateway
    {
        private readonly RazorpayOptions _opts;
        private readonly HttpClient _http;
        private const string BaseUrl = "https://api.razorpay.com/v1";

        public string GatewayName => "Razorpay";

        public RazorpayGateway(IOptions<RazorpayOptions> opts, IHttpClientFactory httpFactory)
        {
            _opts = opts.Value;
            _http = httpFactory.CreateClient("Razorpay");

            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_opts.KeyId}:{_opts.KeySecret}"));
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        }

        public async Task<GatewayOrderResult> CreateOrderAsync(
            GatewayOrderRequest request, CancellationToken ct = default)
        {
            try
            {
                var body = new
                {
                    amount = (long)(request.Amount * 100),  // Razorpay needs paise
                    currency = request.CurrencyCode.ToUpper(),
                    receipt = request.TransactionId.ToString("N")[..20],
                    notes = new
                    {
                        transaction_id = request.TransactionId.ToString(),
                        order_number = request.OrderNumber
                    }
                };

                var response = await _http.PostAsync(
                    $"{BaseUrl}/orders",
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
                    ct);

                var json = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    return new GatewayOrderResult { Success = false, Error = $"Razorpay error: {json}" };

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var gatewayOrderId = root.GetProperty("id").GetString()!;

                return new GatewayOrderResult
                {
                    Success = true,
                    GatewayOrderId = gatewayOrderId,
                    Metadata = new Dictionary<string, string>
                    {
                        ["key_id"] = _opts.KeyId,
                        ["order_id"] = gatewayOrderId,
                        ["amount"] = ((long)(request.Amount * 100)).ToString(),
                        ["currency"] = request.CurrencyCode.ToUpper(),
                        ["name"] = request.BuyerName,
                        ["email"] = request.BuyerEmail,
                        ["contact"] = request.BuyerPhone,
                        ["description"] = $"Payment for order {request.OrderNumber}"
                    }
                };
            }
            catch (Exception ex)
            {
                return new GatewayOrderResult { Success = false, Error = ex.Message };
            }
        }

        public Task<GatewayVerifyResult> VerifyPaymentAsync(
            GatewayVerifyRequest request, CancellationToken ct = default)
        {
            try
            {
                // Razorpay signature: HMAC-SHA256(order_id + "|" + payment_id, secret)
                var payload = $"{request.GatewayOrderId}|{request.GatewayPaymentId}";
                var expectedSignature = ComputeHmacSha256(payload, _opts.KeySecret);

                if (!string.Equals(expectedSignature, request.GatewaySignature,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(new GatewayVerifyResult
                    {
                        Success = false,
                        Error = "Signature verification failed."
                    });
                }

                return Task.FromResult(new GatewayVerifyResult
                {
                    Success = true,
                    GatewayTransactionId = request.GatewayPaymentId,
                    Status = "captured"
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new GatewayVerifyResult { Success = false, Error = ex.Message });
            }
        }

        public async Task<GatewayRefundResult> RefundAsync(
            GatewayRefundRequest request, CancellationToken ct = default)
        {
            try
            {
                var body = new
                {
                    amount = (long)(request.Amount * 100),
                    notes = new { reason = request.Reason }
                };

                var response = await _http.PostAsync(
                    $"{BaseUrl}/payments/{request.GatewayPaymentId}/refund",
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
                    ct);

                var json = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    return new GatewayRefundResult { Success = false, Error = $"Razorpay refund error: {json}" };

                using var doc = JsonDocument.Parse(json);
                return new GatewayRefundResult
                {
                    Success = true,
                    GatewayRefundId = doc.RootElement.GetProperty("id").GetString(),
                    Status = "processed"
                };
            }
            catch (Exception ex)
            {
                return new GatewayRefundResult { Success = false, Error = ex.Message };
            }
        }

        public GatewayWebhookResult? ParseWebhook(
            string rawBody, IReadOnlyDictionary<string, string> headers)
        {
            // Razorpay sends X-Razorpay-Signature header
            if (!headers.TryGetValue("X-Razorpay-Signature", out var signature))
                if (!headers.TryGetValue("x-razorpay-signature", out signature))
                    return null;

            var expectedSignature = ComputeHmacSha256(rawBody, _opts.WebhookSecret);
            if (!string.Equals(expectedSignature, signature, StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;
                var eventType = root.GetProperty("event").GetString() ?? string.Empty;
                var payload = root.GetProperty("payload");
                var paymentEntity = payload
                    .GetProperty("payment")
                    .GetProperty("entity");

                return new GatewayWebhookResult
                {
                    EventType = eventType,
                    GatewayPaymentId = paymentEntity.GetProperty("id").GetString() ?? string.Empty,
                    GatewayOrderId = paymentEntity.GetProperty("order_id").GetString() ?? string.Empty,
                    Amount = paymentEntity.GetProperty("amount").GetDecimal() / 100,
                    Status = paymentEntity.GetProperty("status").GetString(),
                    RawBody = rawBody
                };
            }
            catch
            {
                return null;
            }
        }

        private static string ComputeHmacSha256(string data, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash).ToLower();
        }
    }
}