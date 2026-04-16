using Microsoft.Extensions.Options;
using Payments.Application.DTOs.Gateway;
using Payments.Application.Interfaces;
using Payments.Infrastructure.Gateways.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Payments.Infrastructure.Gateways
{
    /// <summary>
    /// PayU Payment Gateway (formerly PayUMoney / PayU India).
    /// Docs: https://devguide.payu.in/
    /// Flow: POST form to PayU hosted page with SHA512 hash → Buyer pays → PayU POSTs to success/failure URL.
    /// </summary>
    public class PayUGateway : IPaymentGateway
    {
        private readonly PayUOptions _opts;

        public string GatewayName => "PayU";

        private string GatewayUrl => _opts.IsProduction
            ? "https://secure.payu.in/_payment"
            : "https://test.payu.in/_payment";

        public PayUGateway(IOptions<PayUOptions> opts)
        {
            _opts = opts.Value;
        }

        public Task<GatewayOrderResult> CreateOrderAsync(
            GatewayOrderRequest request, CancellationToken ct = default)
        {
            try
            {
                var txnId = request.TransactionId.ToString("N")[..20];
                var amount = request.Amount.ToString("F2");
                var productInfo = $"Order {request.OrderNumber}";

                // PayU hash: SHA512 of key|txnid|amount|productinfo|firstname|email|||||||||||salt
                var hashInput = $"{_opts.MerchantKey}|{txnId}|{amount}|{productInfo}|{request.BuyerName}|{request.BuyerEmail}|||||||||||{_opts.MerchantSalt}";
                var hash = ComputeSha512(hashInput);

                // PayU uses HTML form POST — return the params as metadata for the frontend to build the form
                var metadata = new Dictionary<string, string>
                {
                    ["action"] = GatewayUrl,
                    ["key"] = _opts.MerchantKey,
                    ["txnid"] = txnId,
                    ["amount"] = amount,
                    ["productinfo"] = productInfo,
                    ["firstname"] = request.BuyerName,
                    ["email"] = request.BuyerEmail,
                    ["phone"] = request.BuyerPhone,
                    ["surl"] = request.SuccessUrl,
                    ["furl"] = request.FailureUrl,
                    ["hash"] = hash
                };

                // Build an auto-submit HTML form as the checkout URL (for server-side redirect)
                var formHtml = BuildFormHtml(GatewayUrl, metadata);
                var dataUrl = "data:text/html;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(formHtml));

                return Task.FromResult(new GatewayOrderResult
                {
                    Success = true,
                    GatewayOrderId = txnId,
                    CheckoutUrl = null, // PayU requires form POST, not a GET redirect
                    Metadata = metadata  // Frontend constructs a form and auto-submits
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new GatewayOrderResult { Success = false, Error = ex.Message });
            }
        }

        public Task<GatewayVerifyResult> VerifyPaymentAsync(
            GatewayVerifyRequest request, CancellationToken ct = default)
        {
            try
            {
                // PayU posts back: mihpayid|status|...|key|txnid|amount|productinfo|firstname|email|||||||||||salt
                // Reverse hash: SHA512(salt|status|||||||||||email|firstname|productinfo|amount|txnid|key)
                var callbackParams = request.CallbackParams;

                if (!callbackParams.TryGetValue("status", out var status))
                    return Task.FromResult(new GatewayVerifyResult { Success = false, Error = "Missing status in callback." });

                if (!callbackParams.TryGetValue("mihpayid", out var mihpayid))
                    return Task.FromResult(new GatewayVerifyResult { Success = false, Error = "Missing mihpayid." });

                callbackParams.TryGetValue("hash", out var receivedHash);
                callbackParams.TryGetValue("amount", out var amount);
                callbackParams.TryGetValue("txnid", out var txnid);
                callbackParams.TryGetValue("productinfo", out var productinfo);
                callbackParams.TryGetValue("firstname", out var firstname);
                callbackParams.TryGetValue("email", out var email);
                callbackParams.TryGetValue("udf1", out var udf1);
                callbackParams.TryGetValue("udf2", out var udf2);
                callbackParams.TryGetValue("udf3", out var udf3);
                callbackParams.TryGetValue("udf4", out var udf4);
                callbackParams.TryGetValue("udf5", out var udf5);

                var reverseHashInput = $"{_opts.MerchantSalt}|{status}|{udf5}|{udf4}|{udf3}|{udf2}|{udf1}|{email}|{firstname}|{productinfo}|{amount}|{txnid}|{_opts.MerchantKey}";
                var expectedHash = ComputeSha512(reverseHashInput);

                if (!string.Equals(expectedHash, receivedHash, StringComparison.OrdinalIgnoreCase))
                    return Task.FromResult(new GatewayVerifyResult { Success = false, Error = "Hash verification failed." });

                return Task.FromResult(new GatewayVerifyResult
                {
                    Success = status?.ToUpper() == "SUCCESS",
                    GatewayTransactionId = mihpayid,
                    Status = status,
                    GatewayResponse = System.Text.Json.JsonSerializer.Serialize(callbackParams),
                    Error = status?.ToUpper() != "SUCCESS" ? $"PayU payment status: {status}" : null
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
            // PayU refund via API v2
            try
            {
                var refundUrl = _opts.IsProduction
                    ? "https://info.payu.in/merchant/postservice?form=2"
                    : "https://test.payu.in/merchant/postservice?form=2";

                var command = "cancel_refund_transaction";
                var hashInput = $"{_opts.MerchantKey}|{command}|{request.GatewayPaymentId}|{request.Amount}|{_opts.MerchantSalt}";
                var hash = ComputeSha512(hashInput);

                var formData = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["key"] = _opts.MerchantKey,
                    ["command"] = command,
                    ["var1"] = request.GatewayPaymentId,
                    ["var2"] = request.Amount.ToString("F2"),
                    ["hash"] = hash
                });

                using var http = new HttpClient();
                var response = await http.PostAsync(refundUrl, formData, ct);
                var json = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    return new GatewayRefundResult { Success = false, Error = json };

                using var doc = JsonDocument.Parse(json);
                var msg = doc.RootElement.TryGetProperty("msg", out var msgProp) ? msgProp.GetString() : "";

                return new GatewayRefundResult
                {
                    Success = msg?.ToUpper().Contains("SUCCESS") == true,
                    Status = msg,
                    Error = msg?.ToUpper().Contains("SUCCESS") != true ? msg : null
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
            // PayU posts form data (not JSON) to success/failure URLs
            // Parse as form-encoded
            try
            {
                var parsed = HttpUtility.ParseQueryString(rawBody);
                var status = parsed["status"] ?? string.Empty;
                var mihpayid = parsed["mihpayid"] ?? string.Empty;
                var txnid = parsed["txnid"] ?? string.Empty;
                var amount = decimal.TryParse(parsed["amount"], out var a) ? a : 0;

                // Verify hash
                var hash = parsed["hash"];
                var udf1 = parsed["udf1"] ?? string.Empty;
                var udf2 = parsed["udf2"] ?? string.Empty;
                var udf3 = parsed["udf3"] ?? string.Empty;
                var udf4 = parsed["udf4"] ?? string.Empty;
                var udf5 = parsed["udf5"] ?? string.Empty;
                var email = parsed["email"] ?? string.Empty;
                var firstname = parsed["firstname"] ?? string.Empty;
                var productinfo = parsed["productinfo"] ?? string.Empty;
                var amountStr = parsed["amount"] ?? string.Empty;

                var reverseHashInput = $"{_opts.MerchantSalt}|{status}|{udf5}|{udf4}|{udf3}|{udf2}|{udf1}|{email}|{firstname}|{productinfo}|{amountStr}|{txnid}|{_opts.MerchantKey}";
                var expectedHash = ComputeSha512(reverseHashInput);

                if (!string.Equals(expectedHash, hash, StringComparison.OrdinalIgnoreCase))
                    return null;

                return new GatewayWebhookResult
                {
                    EventType = status?.ToUpper() == "SUCCESS" ? "payment.success" : "payment.failure",
                    GatewayPaymentId = mihpayid,
                    GatewayOrderId = txnid,
                    Amount = amount,
                    Status = status,
                    RawBody = rawBody
                };
            }
            catch
            {
                return null;
            }
        }

        private static string ComputeSha512(string input)
        {
            using var sha = SHA512.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }

        private static string BuildFormHtml(string action, Dictionary<string, string> fields)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body onload=\"document.forms[0].submit()\">");
            sb.Append($"<form method=\"POST\" action=\"{action}\">");
            foreach (var (key, value) in fields)
            {
                if (key == "action") continue;
                sb.Append($"<input type=\"hidden\" name=\"{key}\" value=\"{HttpUtility.HtmlEncode(value)}\" />");
            }
            sb.Append("</form></body></html>");
            return sb.ToString();
        }
    }
}