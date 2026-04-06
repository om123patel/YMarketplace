using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.DTOs.Payouts;
using Payments.Application.Services.Interface;
using Web.API.Controllers;
using Web.API.Extensions;

namespace API.Controllers.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/payouts")]
    public class AdminPayoutsController : BaseController
    {
        private readonly IPayoutService _service;

        public AdminPayoutsController(IPayoutService service)
            => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PayoutFilterRequest filter,
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

        [HttpPost("{id:guid}/process")]
        public async Task<IActionResult> StartProcessing(
            Guid id, [FromBody] string? note,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _service.StartProcessingAsync(id, note, adminId, ct);
            return HandleResult(result);
        }

        [HttpPost("{id:guid}/complete")]
        public async Task<IActionResult> Complete(
            Guid id, [FromBody] ProcessPayoutDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _service.CompleteAsync(id, dto, adminId, ct);
            return HandleResult(result);
        }

        [HttpPost("{id:guid}/fail")]
        public async Task<IActionResult> Fail(
            Guid id, [FromBody] string reason,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _service.FailAsync(id, reason, adminId, ct);
            return HandleResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(
            Guid id, [FromBody] string reason,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _service.CancelAsync(id, reason, adminId, ct);
            return HandleResult(result);
        }
    }
}
