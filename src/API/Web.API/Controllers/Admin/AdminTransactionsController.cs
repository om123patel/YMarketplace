using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.DTOs.Transactions;
using Payments.Application.Services.Interface;
using Web.API.Controllers;
using Web.API.Extensions;

namespace API.Controllers.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/transactions")]
    public class AdminTransactionsController : BaseController
    {
        private readonly ITransactionService _service;

        public AdminTransactionsController(ITransactionService service)
            => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] TransactionFilterRequest filter,
            CancellationToken ct)
        {
            var result = await _service.GetPagedAsync(filter, ct);
            return HandleResult(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        [HttpGet("by-order/{orderId:guid}")]
        public async Task<IActionResult> GetByOrderId(
            Guid orderId, CancellationToken ct)
        {
            var result = await _service.GetByOrderIdAsync(orderId, ct);
            return HandleResult(result);
        }

        [HttpPost("{id:guid}/refund")]
        public async Task<IActionResult> Refund(
            Guid id, [FromBody] RefundTransactionDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _service.RefundAsync(id, dto, adminId, ct);
            return HandleResult(result);
        }
    }
}