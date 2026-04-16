using Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.DTOs.Checkout;
using Payments.Application.Interfaces;
using Payments.Application.Services.Interface;
using Web.API.Extensions;

namespace Web.API.Controllers.Customer
{
    [Route("api/buyer/checkout")]
    [Authorize(Policy = "BuyerOnly")]
    public class CheckoutController : BaseController
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IUserService _userService;

        public CheckoutController(ICheckoutService checkoutService, IUserService userService)
        {
            _checkoutService = checkoutService;
            _userService = userService;
        }

        /// <summary>
        /// Initiate a payment session for an order.
        /// Returns checkout URL (hosted gateways) or SDK options (Razorpay).
        /// </summary>
        [HttpPost("initiate")]
        public async Task<IActionResult> Initiate(
            [FromBody] InitiateCheckoutDto dto,
            CancellationToken ct)
        {
            var buyerId = User.GetUserId();

            // Fetch buyer details for gateway
            var userResult = await _userService.GetByIdAsync(buyerId, ct);
            if (!userResult.IsSuccess)
                return HandleResult(userResult);

            var user = userResult.Value!;

            // In production, fetch order details (amount, orderNumber, categoryId)
            // from an IOrderModuleApi or pass them from the request.
            // For now accept them in the DTO via extended request:
            if (dto is InitiateCheckoutWithOrderDto extDto)
            {
                var result = await _checkoutService.InitiateAsync(
                    dto: extDto,
                    buyerId: buyerId,
                    buyerName: user.FullName,
                    buyerEmail: user.Email,
                    buyerPhone: user.PhoneNumber ?? "9999999999",
                    orderNumber: extDto.OrderNumber,
                    orderAmount: extDto.OrderAmount,
                    categoryId: extDto.CategoryId,
                    ct: ct);

                return HandleResult(result);
            }

            return BadRequest("Use InitiateCheckoutWithOrderDto");
        }

        /// <summary>
        /// Verify payment after the buyer returns from the gateway checkout page.
        /// Called by the frontend after the gateway redirects back.
        /// </summary>
        [HttpPost("verify")]
        public async Task<IActionResult> Verify(
            [FromBody] VerifyPaymentDto dto,
            CancellationToken ct)
        {
            var result = await _checkoutService.VerifyAsync(dto, ct);
            return HandleResult(result);
        }

        /// <summary>
        /// Get available payment gateways.
        /// </summary>
        [HttpGet("gateways")]
        public IActionResult GetGateways(
            [FromServices] IPaymentGatewayFactory factory)
        {
            return Ok(new { gateways = factory.AvailableGateways });
        }
    }

    // Extended DTO that carries order context (avoids cross-module calls in controller)
    public class InitiateCheckoutWithOrderDto : InitiateCheckoutDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
        public int? CategoryId { get; set; }
    }
}