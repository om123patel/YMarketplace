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
    /// PhonePe Payment Gateway (Standard Checkout).
    /// Docs: https://developer.phonepe.com/v1/docs/
    /// Flow: Create order via API → Redirect buyer to checkout URL → Verify via S2S callback.
    /// </summary>
    public class PhonePeGateway : IPaymentGateway
    {
        private readonly PhonePeOptions _opts;
        private readonly HttpClient _http;

        public string GatewayName => "PhonePe";

        private string BaseUrl => _opts.IsProduction
            ? "https://api.phonepe.com/apis/hermes"
            : "https://api-preprod.phonepe.com/apis/pg-sandbox";

        public PhonePeGateway(IOptions<PhonePeOptions> opts, IHttpClientFactory httpFactory)
        {
            _opts = opts.Value;
            _http = httpFactory.CreateClient("PhonePe");
        }

        public async Task<GatewayOrderResult> CreateOrderAsync(
            GatewayOrderRequest request, CancellationToken ct = default)
        {
            try
            {
                var merchantTransactionId = $"MT{request.TransactionId:N}"[..35];

                var payload = new
                {
                    merchantId = _opts.MerchantId,
                    merchantTransactionId,
                    merchantUserId = request.BuyerEmail.Split('@')[0],
                    amount = (long)(request.Amount * 100),  // paise
                    redirectUrl = $"{request.SuccessUrl}?txn={merchantTransactionId}",
                    redirectMode = "REDIRECT",
                    callbackUrl = request.WebhookUrl,
                    mobileNumber = request.BuyerPhone.Replace("+91", "").Replace(" ", "").TrimStart('0'),
                    paymentInstrument = new { type = "PAY_PAGE" }
                };

                var payloadJson = JsonSerializer.Serialize(payload);
                var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson));

                // PhonePe checksum: SHA256(base64payload + "/pg/v1/pay" + saltKey) + "###" + saltIndex
                var stringToHash = payloadBase64 + "/pg/v1/pay" + _opts.SaltKey;
                using var sha = SHA256.Create();
                var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
                var hash = Convert.ToHexString(hashBytes).ToLower();
                var checksum = $"{hash}###{_opts.SaltIndex}";

                _http.DefaultRequestHeaders.Remove("X-VERIFY");
                _http.DefaultRequestHeaders.Add("X-VERIFY", checksum);
                _http.DefaultRequestHeaders.Remove("X-MERCHANT-ID");
                _http.DefaultRequestHeaders.Add("X-MERCHANT-ID", _opts.MerchantId);
                _http.DefaultRequestHeaders.Remove("Content-Type");

                var requestBody = new { request = payloadBase64 };

                var response = await _http.PostAsync(
                    $"{BaseUrl}/pg/v1/pay",
                    new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"),
                    ct);

                var json = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    return new GatewayOrderResult { Success = false, Error = $"PhonePe error: {json}" };

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.GetProperty("success").GetBoolean())
                    return new GatewayOrderResult { Success = false, Error = root.GetProperty("message").GetString() };

                var instrumentResponse = root
                    .GetProperty("data")
                    .GetProperty("instrumentResponse");

                var checkoutUrl = instrumentResponse
                    .GetProperty("redirectInfo")
                    .GetProperty("url")
                    .GetString();

                return new GatewayOrderResult
                {
                    Success = true,
                    GatewayOrderId = merchantTransactionId,
                    CheckoutUrl = checkoutUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        ["merchant_transaction_id"] = merchantTransactionId,
                        ["checkout_url"] = checkoutUrl ?? string.Empty
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
                var merchantTransactionId = request.GatewayOrderId;

                // PhonePe status check: SHA256("/pg/v1/status/{merchantId}/{merchantTransactionId}" + saltKey) + "###" + saltIndex
                var endpoint = $"/pg/v1/status/{_opts.MerchantId}/{merchantTransactionId}";
                var stringToHash = endpoint + _opts.SaltKey;

                using var sha = SHA256.Create();
                var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
                var hash = Convert.ToHexString(hashBytes).ToLower();
                var checksum = $"{hash}###{_opts.SaltIndex}";

                using var statusClient = new HttpClient();
                statusClient.DefaultRequestHeaders.Add("X-VERIFY", checksum);
                statusClient.DefaultRequestHeaders.Add("X-MERCHANT-ID", _opts.MerchantId);

                var response = await statusClient.GetAsync(
                    $"{BaseUrl}{endpoint}", ct);

                var json = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    return new GatewayVerifyResult { Success = false, Error = json };

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var success = root.GetProperty("success").GetBoolean();
                var data = root.GetProperty("data");
                var paymentState = data.GetProperty("state").GetString();

                if (!success || paymentState != "COMPLETED")
                    return new GatewayVerifyResult { Success = false, Status = paymentState, Error = $"Payment state: {paymentState}" };

                var transactionId = data.GetProperty("transactionId").GetString();
                var amount = data.GetProperty("amount").GetDecimal() / 100;

                return new GatewayVerifyResult
                {
                    Success = true,
                    GatewayTransactionId = transactionId,
                    Status = paymentState,
                    AmountVerified = amount,
                    GatewayResponse = json
                };
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
                var refundTransactionId = $"REF_{request.TransactionId:N}"[..35];

                var payload = new
                {
                    merchantId = _opts.MerchantId,
                    merchantUserId = "system",
                    originalTransactionId = request.GatewayPaymentId,
                    merchantTransactionId = refundTransactionId,
                    amount = (long)(request.Amount * 100),
                    callbackUrl = string.Empty
                };

                var payloadJson = JsonSerializer.Serialize(payload);
                var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson));

                var stringToHash = payloadBase64 + "/pg/v1/refund" + _opts.SaltKey;
                using var sha = SHA256.Create();
                var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(stringToHash))).ToLower();
                var checksum = $"{hash}###{_opts.SaltIndex}";

                using var refundClient = new HttpClient();
                refundClient.DefaultRequestHeaders.Add("X-VERIFY", checksum);

                var response = await refundClient.PostAsync(
                    $"{BaseUrl}/pg/v1/refund",
                    new StringContent(JsonSerializer.Serialize(new { request = payloadBase64 }), Encoding.UTF8, "application/json"),
                    ct);

                var json = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    return new GatewayRefundResult { Success = false, Error = json };

                using var doc = JsonDocument.Parse(json);
                var success = doc.RootElement.GetProperty("success").GetBoolean();

                return new GatewayRefundResult
                {
                    Success = success,
                    GatewayRefundId = refundTransactionId,
                    Status = success ? "PENDING" : "FAILED",
                    Error = !success ? doc.RootElement.GetProperty("message").GetString() : null
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
            // PhonePe sends X-VERIFY header: SHA256(rawBody + saltKey) + "###" + saltIndex
            if (!headers.TryGetValue("X-VERIFY", out var verify))
                if (!headers.TryGetValue("x-verify", out verify))
                    return null;

            var parts = verify.Split("###");
            if (parts.Length != 2) return null;

            var stringToHash = rawBody + _opts.SaltKey;
            using var sha = SHA256.Create();
            var expectedHash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(stringToHash))).ToLower();

            if (!string.Equals(expectedHash, parts[0], StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;
                var data = root.GetProperty("data");
                var transactionId = data.GetProperty("transactionId").GetString() ?? string.Empty;
                var merchantTransactionId = data.GetProperty("merchantTransactionId").GetString() ?? string.Empty;
                var paymentState = data.GetProperty("paymentState").GetString() ?? string.Empty;
                var amount = data.GetProperty("amount").GetDecimal() / 100;

                return new GatewayWebhookResult
                {
                    EventType = $"payment.{paymentState.ToLower()}",
                    GatewayPaymentId = transactionId,
                    GatewayOrderId = merchantTransactionId,
                    Amount = amount,
                    Status = paymentState,
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