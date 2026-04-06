using Identity.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.DTOs.Payouts;
using Payments.Application.Services.Interface;
using Web.API.Extensions;

namespace Web.API.Controllers.Seller
{

    [Area("Vendor")]
    [Authorize(Roles = "Seller")]
    [Route("api/vendor/payouts")]
    public class VendorPayoutsController : BaseController
    {
        private readonly IPayoutService _payoutService;
        private readonly ITransactionService _transactionService;

        public VendorPayoutsController(
            IPayoutService payoutService,
            ITransactionService transactionService)
        {
            _payoutService = payoutService;
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyPayouts(CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _payoutService.GetBySellerIdAsync(sellerId, ct);
            return HandleResult(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _payoutService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        [HttpGet("earnings")]
        public async Task<IActionResult> GetEarnings(CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _transactionService
                .GetSellerEarningsSummaryAsync(sellerId, ct);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> RequestPayout(
            [FromBody] RequestPayoutDto dto,
            CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _payoutService.RequestAsync(dto, sellerId, ct);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById),
                    new { id = result.Value!.Id }, result)
                : HandleResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(
            Guid id, [FromBody] string reason,
            CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _payoutService.CancelAsync(id, reason, sellerId, ct);
            return HandleResult(result);
        }
    }

}
