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
    /// Cashfree Payments Gateway.
    /// Docs: https://docs.cashfree.com/
    /// Flow: Create Order via API → Redirect to Cashfree hosted page → Webhook confirms status.
    /// </summary>
    public class CashfreeGateway : IPaymentGateway
    {
        private readonly CashfreeOptions _opts;
        private readonly HttpClient _http;

        public string GatewayName => "Cashfree";

        private string BaseUrl => _opts.Environment == "production"
            ? "https://api.cashfree.com/pg"
            : "https://sandbox.cashfree.com/pg";

        public CashfreeGateway(IOptions<CashfreeOptions> opts, IHttpClientFactory httpFactory)
        {
            _opts = opts.Value;
            _http = httpFactory.CreateClient("Cashfree");
            _http.DefaultRequestHeaders.Add("x-api-version", "2023-08-01");
            _http.DefaultRequestHeaders.Add("x-client-id", _opts.AppId);
            _http.DefaultRequestHeaders.Add("x-client-secret", _opts.SecretKey);
        }

        public async Task<GatewayOrderResult> CreateOrderAsync(
            GatewayOrderRequest request, CancellationToken ct = default)
        {
            try
            {
                var body = new
                {
                    order_id = $"ORD_{request.TransactionId:N}",
                    order_amount = request.Amount,
                    order_currency = request.CurrencyCode.ToUpper(),
                    customer_details = new
                    {
                        customer_id = request.TransactionId.ToString("N")[..20],
                        customer_name = request.BuyerName,
                        customer_email = request.BuyerEmail,
                        customer_phone = request.BuyerPhone
                    },
                    order_meta = new
                    {
                        return_url = $"{request.SuccessUrl}?order_id={{order_id}}&transaction_id={request.TransactionId}",
                        notify_url = request.WebhookUrl
                    }
                };

                var response = await _http.PostAsync(
                    $"{BaseUrl}/orders",
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
                    ct);

                var json = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    return new GatewayOrderResult { Success = false, Error = $"Cashfree error: {json}" };

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var gatewayOrderId = root.GetProperty("order_id").GetString()!;
                var paymentSessionId = root.GetProperty("payment_session_id").GetString()!;

                // Cashfree supports both redirect and SDK flow
                var checkoutUrl = _opts.Environment == "production"
                    ? $"https://payments.cashfree.com/order/#payment_session_id={paymentSessionId}"
                    : $"https://sandbox.cashfree.com/pg/#payment_session_id={paymentSessionId}";

                return new GatewayOrderResult
                {
                    Success = true,
                    GatewayOrderId = gatewayOrderId,
                    CheckoutUrl = checkoutUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        ["payment_session_id"] = paymentSessionId,
                        ["order_id"] = gatewayOrderId,
                        ["environment"] = _opts.Environment
                    }
                };
            }
            catch (Exception ex)
            {
                return new GatewayOrderResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<GatewayVerifyResult> VerifyPaymentAsync(
            GatewayVerifyRequest request, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.GetAsync(
                    $"{BaseUrl}/orders/{request.GatewayOrderId}/payments", ct);

                var json = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    return new GatewayVerifyResult { Success = false, Error = json };

                using var doc = JsonDocument.Parse(json);
                var payments = doc.RootElement;

                // Find the latest successful payment
                foreach (var payment in payments.EnumerateArray())
                {
                    var status = payment.GetProperty("payment_status").GetString();
                    if (status == "SUCCESS")
                    {
                        return new GatewayVerifyResult
                        {
                            Success = true,
                            GatewayTransactionId = payment.GetProperty("cf_payment_id").GetString(),
                            Status = "SUCCESS",
                            AmountVerified = payment.GetProperty("payment_amount").GetDecimal(),
                            GatewayResponse = json
                        };
                    }
                }

                return new GatewayVerifyResult { Success = false, Error = "Payment not successful.", Status = "PENDING" };
            }
            catch (Exception ex)
            {
                return new GatewayVerifyResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<GatewayRefundResult> RefundAsync(
            GatewayRefundRequest request, CancellationToken ct = default)
        {
            try
            {
                var body = new
                {
                    refund_amount = request.Amount,
                    refund_id = $"REF_{request.TransactionId:N}",
                    refund_note = request.Reason
                };

                var response = await _http.PostAsync(
                    $"{BaseUrl}/orders/{request.GatewayPaymentId}/refunds",
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
                    ct);

                var json = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    return new GatewayRefundResult { Success = false, Error = json };

                using var doc = JsonDocument.Parse(json);
                return new GatewayRefundResult
                {
                    Success = true,
                    GatewayRefundId = doc.RootElement.GetProperty("refund_id").GetString(),
                    Status = doc.RootElement.GetProperty("refund_status").GetString()
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
            // Cashfree: x-webhook-signature, x-webhook-timestamp
            if (!headers.TryGetValue("x-webhook-signature", out var signature))
                if (!headers.TryGetValue("X-Webhook-Signature", out signature))
                    return null;

            if (!headers.TryGetValue("x-webhook-timestamp", out var timestamp))
                if (!headers.TryGetValue("X-Webhook-Timestamp", out timestamp))
                    return null;

            // Cashfree signature: HMAC-SHA256(timestamp + rawBody, secretKey)
            var signedData = timestamp + rawBody;
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_opts.WebhookSecret));
            var expected = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signedData)));

            if (!string.Equals(expected, signature, StringComparison.Ordinal))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;
                var eventType = root.GetProperty("type").GetString() ?? string.Empty;
                var data = root.GetProperty("data");
                var payment = data.GetProperty("payment");
                var order = data.GetProperty("order");

                return new GatewayWebhookResult
                {
                    EventType = eventType,
                    GatewayPaymentId = payment.GetProperty("cf_payment_id").ToString(),
                    GatewayOrderId = order.GetProperty("order_id").GetString() ?? string.Empty,
                    Amount = payment.GetProperty("payment_amount").GetDecimal(),
                    Status = payment.GetProperty("payment_status").GetString(),
                    RawBody = rawBody
                };
            }
            catch
            {
                return null;
            }
        }
    }
}