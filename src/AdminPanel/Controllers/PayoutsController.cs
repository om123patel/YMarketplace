using AdminPanel.Dtos.Payments;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Common;
using AdminPanel.ViewModels.Grid;
using AdminPanel.ViewModels.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("payments/payouts")]
    public class PayoutsController : Controller
    {
        private readonly IPayoutApiClient _client;
        private readonly AuthTokenService _tokens;

        public PayoutsController(IPayoutApiClient client, AuthTokenService tokens)
        {
            _client = client;
            _tokens = tokens;
        }

        // GET /payments/payouts
        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? status, string? search, int page = 1,
            CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.GetPayoutsAsync(token, page, 20, status, search);

            var vm = new PayoutIndexViewModel
            {
                Items = result?.Data?.Items.Select(p => new PayoutListItemViewModel
                {
                    Id = p.Id,
                    SellerId = p.SellerId,
                    SellerName = p.SellerName,
                    Amount = p.Amount,
                    CurrencyCode = p.CurrencyCode,
                    Status = p.Status,
                    PaymentMethod = !string.IsNullOrWhiteSpace(p.UpiId)
                        ? $"UPI: {p.UpiId}"
                        : p.BankAccountNumber is not null
                            ? $"Bank: ****{p.BankAccountNumber[^Math.Min(4, p.BankAccountNumber.Length)..]}"
                            : null,
                    CompletedAt = p.CompletedAt,
                    CreatedAt = p.CreatedAt
                }).ToList() ?? [],
                Page = result?.Data?.Page ?? page,
                PageSize = result?.Data?.PageSize ?? 20,
                TotalCount = result?.Data?.TotalCount ?? 0,
                Search = search,
                StatusFilter = status,
                StatusOptions = new[]
                {
                    "Pending", "Processing", "Completed", "Failed", "Cancelled"
                }.Select(s => new FilterOption(s, s)).ToList(),
                RouteValues = new()
                {
                    ["status"] = status,
                    ["search"] = search
                }
            };

            vm.BuildRouteData(new() { ["status"] = status, ["search"] = search });
            return View(vm);
        }

        // GET /payments/payouts/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.GetByIdAsync(token, id);

            if (result?.Data is null)
            {
                TempData["Error"] = "Payout not found.";
                return RedirectToAction(nameof(Index));
            }

            var p = result.Data;
            var vm = new PayoutDetailViewModel
            {
                Id = p.Id,
                SellerId = p.SellerId,
                SellerName = p.SellerName,
                Amount = p.Amount,
                CurrencyCode = p.CurrencyCode,
                Status = p.Status,
                BankAccountNumber = p.BankAccountNumber,
                BankIfscCode = p.BankIfscCode,
                BankAccountName = p.BankAccountName,
                UpiId = p.UpiId,
                AdminNote = p.AdminNote,
                FailureReason = p.FailureReason,
                GatewayReference = p.GatewayReference,
                ProcessedAt = p.ProcessedAt,
                CompletedAt = p.CompletedAt,
                FailedAt = p.FailedAt,
                CancelledAt = p.CancelledAt,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };

            return View(vm);
        }

        // POST /payments/payouts/{id}/process
        [HttpPost("{id:guid}/process")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(
            Guid id, string? adminNote, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.StartProcessingAsync(token, id, adminNote);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Payout marked as processing."
                    : result?.Error ?? "Failed to update payout status.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST /payments/payouts/{id}/complete
        [HttpPost("{id:guid}/complete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(
            Guid id, string gatewayReference, string? adminNote,
            CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.CompleteAsync(token, id,
                new ProcessPayoutRequest
                {
                    GatewayReference = gatewayReference,
                    AdminNote = adminNote
                });

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Payout completed successfully."
                    : result?.Error ?? "Failed to complete payout.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST /payments/payouts/{id}/fail
        [HttpPost("{id:guid}/fail")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fail(
            Guid id, string reason, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.FailAsync(token, id, reason);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Payout marked as failed."
                    : result?.Error ?? "Failed to update payout.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST /payments/payouts/{id}/cancel
        [HttpPost("{id:guid}/cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(
            Guid id, string reason, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.CancelAsync(token, id, reason);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Payout cancelled."
                    : result?.Error ?? "Failed to cancel payout.";

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}