using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.DTOs.Orders;
using Orders.Application.Services.Interface;
using Web.API.Extensions;

namespace Web.API.Controllers.Seller
{
    [Route("api/seller/orders")]
    [Authorize(Policy = "SellerOnly")]
    public class VendorOrdersController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IDisputeService _disputeService;

        public VendorOrdersController(
            IOrderService orderService,
            IDisputeService disputeService)
        {
            _orderService = orderService;
            _disputeService = disputeService;
        }

        // GET api/seller/orders
        [HttpGet]
        public async Task<IActionResult> GetMyOrders(
            int page = 1, int pageSize = 20,
            string? status = null, CancellationToken ct = default)
        {
            // StoreId from JWT claim
            var storeIdClaim = User.FindFirst("storeId")?.Value;
            if (!Guid.TryParse(storeIdClaim, out var storeId))
                return Unauthorized();

            var result = await _orderService.GetStoreOrdersAsync(
                storeId, page, pageSize, status, ct);
            return HandleResult(result);
        }

        // GET api/seller/orders/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _orderService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // PATCH api/seller/orders/{id}/confirm
        [HttpPatch("{id:guid}/confirm")]
        public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _orderService.ConfirmAsync(id, sellerId, ct);
            return HandleResult(result);
        }

        // PATCH api/seller/orders/{id}/ship
        [HttpPatch("{id:guid}/ship")]
        public async Task<IActionResult> Ship(
            Guid id, [FromBody] ShipOrderDto dto, CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _orderService.ShipAsync(id, dto, sellerId, ct);
            return HandleResult(result);
        }

        // PATCH api/seller/orders/{id}/deliver
        [HttpPatch("{id:guid}/deliver")]
        public async Task<IActionResult> MarkDelivered(Guid id, CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _orderService.MarkDeliveredAsync(id, sellerId, ct);
            return HandleResult(result);
        }

        // POST api/seller/orders/disputes/{id}/respond
        [HttpPost("disputes/{id:guid}/respond")]
        public async Task<IActionResult> RespondToDispute(
            Guid id, [FromBody] SellerDisputeResponse req, CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _disputeService.SubmitSellerResponseAsync(
                id, req.Response, sellerId, ct);
            return HandleResult(result);
        }
    }

    public record SellerDisputeResponse(string Response);
}