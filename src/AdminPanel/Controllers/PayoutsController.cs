using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Grid;
using AdminPanel.ViewModels.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.DTOs.Payouts;

namespace AdminPanel.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("payments/payouts")]
    public class PayoutsController : Controller
    {
        private readonly IPayoutApiClient _client;

        public PayoutsController(IPayoutApiClient client)
            => _client = client;

        // GET /payments/payouts
        public async Task<IActionResult> Index(
            string? status, string? search, int page = 1)
        {
            var filter = new PayoutFilterRequest
            {
                Status = status,
                Search = search,
                Page = page,
                PageSize = 20
            };

            var paged = await _client.GetPagedAsync(filter);

            var vm = new PayoutIndexViewModel
            {
                Items = paged?.Items.Select(MapToListItem).ToList() ?? [],
                Page = paged?.Page ?? 1,
                PageSize = paged?.PageSize ?? 20,
                TotalCount = paged?.TotalCount ?? 0,
                Search = search,
                StatusFilter = status,
                StatusOptions = BuildStatusOptions(status),
                RouteValues = new Dictionary<string, string?>
                {
                    ["status"] = status,
                    ["search"] = search
                }
            };

            return View(vm);
        }

        // GET /payments/payouts/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Detail(Guid id)
        {
            var dto = await _client.GetByIdAsync(id);
            if (dto is null) return NotFound();

            var vm = new PayoutDetailViewModel
            {
                Id = dto.Id,
                SellerId = dto.SellerId,
                SellerName = dto.SellerName,
                Amount = dto.Amount,
                CurrencyCode = dto.CurrencyCode,
                Status = dto.Status,
                BankAccountNumber = dto.BankAccountNumber,
                BankIfscCode = dto.BankIfscCode,
                BankAccountName = dto.BankAccountName,
                UpiId = dto.UpiId,
                AdminNote = dto.AdminNote,
                FailureReason = dto.FailureReason,
                GatewayReference = dto.GatewayReference,
                ProcessedAt = dto.ProcessedAt,
                CompletedAt = dto.CompletedAt,
                FailedAt = dto.FailedAt,
                CancelledAt = dto.CancelledAt,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };

            return View(vm);
        }

        // POST /payments/payouts/{id}/process
        [HttpPost("{id:guid}/process")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(Guid id, string? adminNote)
        {
            var result = await _client.StartProcessingAsync(id, adminNote);
            TempData[result is not null ? "SuccessMessage" : "ErrorMessage"] =
                result is not null
                    ? "Payout marked as processing."
                    : "Failed to update payout status.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST /payments/payouts/{id}/complete
        [HttpPost("{id:guid}/complete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(
            Guid id, string gatewayReference, string? adminNote)
        {
            var result = await _client.CompleteAsync(id, new ProcessPayoutDto
            {
                GatewayReference = gatewayReference,
                AdminNote = adminNote
            });

            TempData[result is not null ? "SuccessMessage" : "ErrorMessage"] =
                result is not null
                    ? "Payout completed successfully."
                    : "Failed to complete payout.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST /payments/payouts/{id}/fail
        [HttpPost("{id:guid}/fail")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fail(Guid id, string reason)
        {
            var result = await _client.FailAsync(id, reason);
            TempData[result is not null ? "SuccessMessage" : "ErrorMessage"] =
                result is not null
                    ? "Payout marked as failed."
                    : "Failed to update payout.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST /payments/payouts/{id}/cancel
        [HttpPost("{id:guid}/cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id, string reason)
        {
            var result = await _client.CancelAsync(id, reason);
            TempData[result is not null ? "SuccessMessage" : "ErrorMessage"] =
                result is not null
                    ? "Payout cancelled."
                    : "Failed to cancel payout.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        // ── Helpers ────────────────────────────────────────────
        private static PayoutListItemViewModel MapToListItem(
            PayoutListItemDto dto)
        {
            var paymentMethod = !string.IsNullOrWhiteSpace(dto.UpiId)
                ? $"UPI: {dto.UpiId}"
                : dto.BankAccountNumber is not null
                    ? $"Bank: ****{dto.BankAccountNumber[^4..]}"
                    : null;

            return new PayoutListItemViewModel
            {
                Id = dto.Id,
                SellerId = dto.SellerId,
                SellerName = dto.SellerName,
                Amount = dto.Amount,
                CurrencyCode = dto.CurrencyCode,
                Status = dto.Status,
                PaymentMethod = paymentMethod,
                CompletedAt = dto.CompletedAt,
                CreatedAt = dto.CreatedAt
            };
        }

        private static List<FilterOption> BuildStatusOptions(string? current)
        {
            string[] statuses = [
                "Pending", "Processing", "Completed", "Failed", "Cancelled"
            ];
            return statuses.Select(s => new FilterOption
            {
                Value = s,
                Label = s,
                Selected = s == current
            }).ToList();
        }
    }
}
