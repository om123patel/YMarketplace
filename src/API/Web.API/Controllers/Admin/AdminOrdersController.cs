using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.DTOs.Dispute;
using Orders.Application.DTOs.Orders;
using Orders.Application.Services.Interface;
using Web.API.Extensions;

namespace Web.API.Controllers.Admin
{
    [Route("api/admin/orders")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminOrdersController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IDisputeService _disputeService;

        public AdminOrdersController(
            IOrderService orderService,
            IDisputeService disputeService)
        {
            _orderService = orderService;
            _disputeService = disputeService;
        }

        // GET api/admin/orders
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] OrderFilterRequest filter, CancellationToken ct)
        {
            var result = await _orderService.GetPagedAsync(filter, ct);
            return HandleResult(result);
        }

        // GET api/admin/orders/{id}
        [HttpGet("{id:guid}", Name = "GetOrderById")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _orderService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/orders/{id}/cancel
        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(
            Guid id, [FromBody] CancelOrderDto dto, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _orderService.AdminCancelAsync(id, dto, adminId, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/orders/{id}/refund
        [HttpPatch("{id:guid}/refund")]
        public async Task<IActionResult> ForceRefund(Guid id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _orderService.ForceRefundAsync(id, adminId, ct);
            return HandleResult(result);
        }

        // GET api/admin/orders/disputes
        [HttpGet("disputes")]
        public async Task<IActionResult> GetDisputes(
            int page = 1, int pageSize = 20,
            string? status = null, CancellationToken ct = default)
        {
            var result = await _disputeService.GetPagedAsync(page, pageSize, status, ct);
            return HandleResult(result);
        }

        // GET api/admin/orders/disputes/{id}
        [HttpGet("disputes/{id:guid}")]
        public async Task<IActionResult> GetDisputeById(Guid id, CancellationToken ct)
        {
            var result = await _disputeService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/orders/disputes/{id}/resolve
        [HttpPatch("disputes/{id:guid}/resolve")]
        public async Task<IActionResult> ResolveDispute(
            Guid id, [FromBody] ResolveDisputeDto dto, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _disputeService.ResolveAsync(id, dto, adminId, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/orders/disputes/{id}/escalate
        [HttpPatch("disputes/{id:guid}/escalate")]
        public async Task<IActionResult> EscalateDispute(
            Guid id, [FromBody] EscalateDisputeRequest req, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _disputeService.EscalateAsync(id, req.Note, adminId, ct);
            return HandleResult(result);
        }
    }

    public record EscalateDisputeRequest(string Note);
}