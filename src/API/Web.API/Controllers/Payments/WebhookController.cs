using Microsoft.AspNetCore.Mvc;
using Payments.Application.Services.Interface;

namespace Web.API.Controllers.Payments
{
    /// <summary>
    /// Receives server-to-server webhook notifications from payment gateways.
    /// These endpoints must be publicly accessible (no auth) and whitelisted on gateway dashboards.
    /// </summary>
    [Route("api/payments/webhook")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            ICheckoutService checkoutService,
            ILogger<WebhookController> logger)
        {
            _checkoutService = checkoutService;
            _logger = logger;
        }

        [HttpPost("razorpay")]
        public Task<IActionResult> Razorpay() => HandleWebhook("Razorpay");

        [HttpPost("cashfree")]
        public Task<IActionResult> Cashfree() => HandleWebhook("Cashfree");

        [HttpPost("payu")]
        public Task<IActionResult> PayU() => HandleWebhook("PayU");

        [HttpPost("phonepe")]
        public Task<IActionResult> PhonePe() => HandleWebhook("PhonePe");

        [HttpPost("paytm")]
        public Task<IActionResult> Paytm() => HandleWebhook("Paytm");

        private async Task<IActionResult> HandleWebhook(string gatewayName)
        {
            // Read raw body — important for signature verification
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // Collect all request headers as a dict
            var headers = Request.Headers
                .ToDictionary(h => h.Key, h => h.Value.ToString(),
                    StringComparer.OrdinalIgnoreCase);

            _logger.LogInformation(
                "Webhook received from {Gateway}: {Body}",
                gatewayName, rawBody[..Math.Min(200, rawBody.Length)]);

            var result = await _checkoutService.HandleWebhookAsync(
                gatewayName, rawBody, headers);

            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "Webhook processing failed for {Gateway}: {Error}",
                    gatewayName, result.Error);

                // Return 200 even on failure (gateways will retry on non-200)
                // Log the error but don't expose internal details
            }

            // Gateways expect 200 OK — return even for validation failures to prevent retries
            return Ok(new { status = "received" });
        }
    }
}