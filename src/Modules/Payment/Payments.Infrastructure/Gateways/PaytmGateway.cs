using Microsoft.Extensions.Options;
using Payments.Application.DTOs.Gateway;
using Payments.Application.Interfaces;
using Payments.Infrastructure.Gateways.Payments.Infrastructure.Gateways;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Payments.Infrastructure.Gateways
{
    /// <summary>
    /// Paytm Payment Gateway.
    /// Docs: https://developer.paytm.com/docs/
    /// Flow: Initiate transaction via API → Redirect to Paytm hosted page → Verify via webhook or status API.
    /// Note: Paytm PG uses HMAC-SHA256 with specific param ordering for checksums.
    /// </summary>
    public class PaytmGateway : IPaymentGateway
    {
        private readonly PaytmOptions _opts;
        private readonly HttpClient _http;

        public string GatewayName => "Paytm";

        private string BaseUrl => _opts.IsProduction
            ? "https://securegw.paytm.in"
            : "https://securegw-stage.paytm.in";

        public PaytmGateway(IOptions<PaytmOptions> opts, IHttpClientFactory httpFactory)
        {
            _opts = opts.Value;
            _http = httpFactory.CreateClient("Paytm");
        }

        public async Task<GatewayOrderResult> CreateOrderAsync(
            GatewayOrderRequest request, CancellationToken ct = default)
        {
            try
            {
                var orderId = $"ORD_{request.TransactionId:N}"[..50];

                // Step 1: Get transaction token
                var tokenBody = new
                {
                    body = new
                    {
                        requestType = "Payment",
                        mid = _opts.MerchantId,
                        websiteName = _opts.IsProduction ? "WEBPROD" : "WEBSTAGING",
                        orderId,
                        txnAmount = new { value = request.Amount.ToString("F2"), currency = "INR" },
                        userInfo = new { custId = request.BuyerEmail, mobile = request.BuyerPhone, email = request.BuyerEmail },
                        callbackUrl = $"{request.SuccessUrl}?order_id={orderId}&transaction_id={request.TransactionId}"
                    }
                };

                var tokenBodyJson = JsonSerializer.Serialize(tokenBody.body);
                var checksum = GeneratePaytmChecksum(tokenBodyJson, _opts.MerchantKey);

                var fullBody = new
                {
                    body = tokenBody.body,
                    head = new { signature = checksum }
                };

                var response = await _http.PostAsync(
                    $"{BaseUrl}/theia/api/v1/initiateTransaction?mid={_opts.MerchantId}&orderId={orderId}",
                    new StringContent(JsonSerializer.Serialize(fullBody), Encoding.UTF8, "application/json"),
                    ct);

                var json = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    return new GatewayOrderResult { Success = false, Error = $"Paytm error: {json}" };

                using var doc = JsonDocument.Parse(json);
                var body = doc.RootElement.GetProperty("body");
                var resultInfo = body.GetProperty("resultInfo");
                var resultCode = resultInfo.GetProperty("resultCode").GetString();

                if (resultCode != "0000")
                    return new GatewayOrderResult { Success = false, Error = resultInfo.GetProperty("resultMsg").GetString() };

                var txnToken = body.GetProperty("txnToken").GetString()!;
                var checkoutUrl = $"{BaseUrl}/theia/api/v1/showPaymentPage?mid={_opts.MerchantId}&orderId={orderId}&txnToken={txnToken}";

                return new GatewayOrderResult
                {
                    Success = true,
                    GatewayOrderId = orderId,
                    CheckoutUrl = checkoutUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        ["txn_token"] = txnToken,
                        ["order_id"] = orderId,
                        ["mid"] = _opts.MerchantId
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
                var body = new { mid = _opts.MerchantId, orderId = request.GatewayOrderId };
                var bodyJson = JsonSerializer.Serialize(body);
                var checksum = GeneratePaytmChecksum(bodyJson, _opts.MerchantKey);

                var fullBody = new
                {
                    body,
                    head = new { signature = checksum }
                };

                var response = await _http.PostAsync(
                    $"{BaseUrl}/v3/order/status",
                    new StringContent(JsonSerializer.Serialize(fullBody), Encoding.UTF8, "application/json"),
                    ct);

                var json = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    return new GatewayVerifyResult { Success = false, Error = json };

                using var doc = JsonDocument.Parse(json);
                var respBody = doc.RootElement.GetProperty("body");
                var resultInfo = respBody.GetProperty("resultInfo");
                var resultCode = resultInfo.GetProperty("resultCode").GetString();
                var txnId = respBody.TryGetProperty("txnId", out var txnProp) ? txnProp.GetString() : null;
                var txnAmount = respBody.TryGetProperty("txnAmount", out var amtProp) ? amtProp.GetString() : null;

                bool isSuccess = resultCode == "01";

                return new GatewayVerifyResult
                {
                    Success = isSuccess,
                    GatewayTransactionId = txnId,
                    Status = resultCode,
                    AmountVerified = decimal.TryParse(txnAmount, out var amt) ? amt : null,
                    GatewayResponse = json,
                    Error = !isSuccess ? resultInfo.GetProperty("resultMsg").GetString() : null
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
                var refundId = $"REF_{request.TransactionId:N}"[..50];
                var body = new
                {
                    mid = _opts.MerchantId,
                    txnId = request.GatewayPaymentId,
                    refundAmount = request.Amount.ToString("F2"),
                    refId = refundId
                };

                var bodyJson = JsonSerializer.Serialize(body);
                var checksum = GeneratePaytmChecksum(bodyJson, _opts.MerchantKey);

                var fullBody = new { body, head = new { signature = checksum } };

                var response = await _http.PostAsync(
                    $"{BaseUrl}/v2/refund/apply",
                    new StringContent(JsonSerializer.Serialize(fullBody), Encoding.UTF8, "application/json"),
                    ct);

                var json = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    return new GatewayRefundResult { Success = false, Error = json };

                using var doc = JsonDocument.Parse(json);
                var respBody = doc.RootElement.GetProperty("body");
                var resultCode = respBody.GetProperty("resultInfo").GetProperty("resultCode").GetString();

                return new GatewayRefundResult
                {
                    Success = resultCode == "10",
                    GatewayRefundId = refundId,
                    Status = resultCode == "10" ? "PENDING" : "FAILED",
                    Error = resultCode != "10" ? respBody.GetProperty("resultInfo").GetProperty("resultMsg").GetString() : null
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
            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;

                // Paytm webhook sends CHECKSUMHASH in body
                var checksumReceived = root.TryGetProperty("CHECKSUMHASH", out var csProp)
                    ? csProp.GetString() : null;

                if (string.IsNullOrEmpty(checksumReceived))
                    return null;

                // Build params dict excluding CHECKSUMHASH for verification
                var paramDict = new SortedDictionary<string, string>(StringComparer.Ordinal);
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Name != "CHECKSUMHASH")
                        paramDict[prop.Name] = prop.Value.GetString() ?? string.Empty;
                }

                // Verify checksum
                if (!VerifyPaytmChecksum(paramDict, _opts.WebhookKey, checksumReceived))
                    return null;

                var status = root.TryGetProperty("STATUS", out var sProp) ? sProp.GetString() : string.Empty;
                var txnId = root.TryGetProperty("TXNID", out var tidProp) ? tidProp.GetString() : string.Empty;
                var orderId = root.TryGetProperty("ORDERID", out var oidProp) ? oidProp.GetString() : string.Empty;
                var amountStr = root.TryGetProperty("TXNAMOUNT", out var amtProp) ? amtProp.GetString() : "0";

                return new GatewayWebhookResult
                {
                    EventType = status == "TXN_SUCCESS" ? "payment.success" : "payment.failure",
                    GatewayPaymentId = txnId ?? string.Empty,
                    GatewayOrderId = orderId ?? string.Empty,
                    Amount = decimal.TryParse(amountStr, out var amt) ? amt : 0,
                    Status = status,
                    RawBody = rawBody
                };
            }
            catch
            {
                return null;
            }
        }

        private static string GeneratePaytmChecksum(string body, string merchantKey)
        {
            // Paytm uses: HMAC-SHA256(body, merchantKey), base64 encoded
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchantKey));
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));
        }

        private static bool VerifyPaytmChecksum(
            SortedDictionary<string, string> paramDict,
            string merchantKey, string checksum)
        {
            // Build verification string from sorted params
            var paramStr = string.Join("|", paramDict.Values.Select(v => v ?? "null"));
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchantKey));
            var expected = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(paramStr)));
            return string.Equals(expected, checksum, StringComparison.Ordinal);
        }
    }
}