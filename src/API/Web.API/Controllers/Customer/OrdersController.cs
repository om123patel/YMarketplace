using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.DTOs.Dispute;
using Orders.Application.DTOs.Orders;
using Orders.Application.Services.Interface;
using Web.API.Extensions;

namespace Web.API.Controllers.Customer
{
    [Route("api/buyer/orders")]
    [Authorize(Policy = "BuyerOnly")]
    public class OrdersController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IDisputeService _disputeService;

        public OrdersController(
            IOrderService orderService,
            IDisputeService disputeService)
        {
            _orderService = orderService;
            _disputeService = disputeService;
        }

        // GET api/buyer/orders
        [HttpGet]
        public async Task<IActionResult> GetMyOrders(
     int page = 1, int pageSize = 20,
     string? status = null,
     DateTime? from = null,
     DateTime? to = null,
     string? search = null,
     CancellationToken ct = default)
        {
            var buyerId = User.GetUserId();
            var filter = new OrderFilterRequest
            {
                Page = page,
                PageSize = pageSize,
                BuyerId = buyerId,
                Status = status,
                CreatedFrom = from,
                CreatedTo = to,
                Search = search
            };
            var result = await _orderService.GetPagedAsync(filter, ct);
            return HandleResult(result);
        }

        // GET api/buyer/orders/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _orderService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // POST api/buyer/orders
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(
            [FromBody] PlaceOrderDto dto, CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var result = await _orderService.PlaceOrderAsync(dto, buyerId, ct);
            return HandleCreated(result, "GetOrderById", new { id = result.Value?.Id });
        }

        // PATCH api/buyer/orders/{id}/cancel
        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(
            Guid id, [FromBody] CancelOrderDto dto, CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var result = await _orderService.CancelByBuyerAsync(id, dto, buyerId, ct);
            return HandleResult(result);
        }

        // POST api/buyer/orders/{id}/disputes
        [HttpPost("{id:guid}/disputes")]
        public async Task<IActionResult> OpenDispute(
            Guid id, [FromBody] OpenDisputeDto dto, CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var result = await _disputeService.OpenDisputeAsync(id, dto, buyerId, ct);
            return HandleResult(result);
        }
    }
}