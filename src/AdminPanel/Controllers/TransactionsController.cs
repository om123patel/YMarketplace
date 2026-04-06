using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Grid;
using AdminPanel.ViewModels.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.DTOs.Transactions;

namespace AdminPanel.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("payments/transactions")]
    public class TransactionsController : Controller
    {
        private readonly ITransactionApiClient _client;

        public TransactionsController(ITransactionApiClient client)
            => _client = client;

        // GET /payments/transactions
        public async Task<IActionResult> Index(
            string? status, string? method,
            string? dateFrom, string? dateTo,
            string? search, int page = 1)
        {
            var filter = new TransactionFilterRequest
            {
                Status = status,
                Method = method,
                DateFrom = dateFrom is not null ? DateTime.Parse(dateFrom) : null,
                DateTo = dateTo is not null ? DateTime.Parse(dateTo) : null,
                Search = search,
                Page = page,
                PageSize = 20,
                SortBy = "createdAt",
                SortDirection = "desc"
            };

            var paged = await _client.GetPagedAsync(filter);

            var vm = new TransactionIndexViewModel
            {
                Items = paged?.Items.Select(MapToListItem).ToList() ?? [],
                Page = paged?.Page ?? 1,
                PageSize = paged?.PageSize ?? 20,
                TotalCount = paged?.TotalCount ?? 0,
                Search = search,
                SortBy = "createdAt",
                SortDirection = "desc",
                StatusFilter = status,
                MethodFilter = method,
                DateFromFilter = dateFrom,
                DateToFilter = dateTo,
                StatusOptions = BuildStatusOptions(status),
                MethodOptions = BuildMethodOptions(method),
                RouteValues = new Dictionary<string, string?>
                {
                    ["status"] = status,
                    ["method"] = method,
                    ["dateFrom"] = dateFrom,
                    ["dateTo"] = dateTo,
                    ["search"] = search
                }
            };

            return View(vm);
        }

        // GET /payments/transactions/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Detail(Guid id)
        {
            var dto = await _client.GetByIdAsync(id);
            if (dto is null) return NotFound();

            var vm = new TransactionDetailViewModel
            {
                Id = dto.Id,
                OrderId = dto.OrderId,
                BuyerId = dto.BuyerId,
                SellerId = dto.SellerId,
                StoreId = dto.StoreId,
                Amount = dto.Amount,
                CommissionAmount = dto.CommissionAmount,
                SellerAmount = dto.SellerAmount,
                CurrencyCode = dto.CurrencyCode,
                Status = dto.Status,
                Method = dto.Method,
                GatewayTransactionId = dto.GatewayTransactionId,
                GatewayProvider = dto.GatewayProvider,
                RefundedAmount = dto.RefundedAmount,
                RefundReason = dto.RefundReason,
                RefundedAt = dto.RefundedAt,
                CompletedAt = dto.CompletedAt,
                FailedAt = dto.FailedAt,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };

            return View(vm);
        }

        // POST /payments/transactions/{id}/refund
        [HttpPost("{id:guid}/refund")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(
            Guid id, decimal amount, string reason)
        {
            var result = await _client.RefundAsync(id, new RefundTransactionDto
            {
                Amount = amount,
                Reason = reason
            });

            TempData[result is not null ? "SuccessMessage" : "ErrorMessage"] =
                result is not null
                    ? "Refund processed successfully."
                    : "Failed to process refund. Please try again.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        // ── Helpers ────────────────────────────────────────────
        private static TransactionListItemViewModel MapToListItem(
            TransactionListItemDto dto)
            => new()
            {
                Id = dto.Id,
                OrderId = dto.OrderId,
                SellerId = dto.SellerId,
                Amount = dto.Amount,
                CommissionAmount = dto.CommissionAmount,
                SellerAmount = dto.SellerAmount,
                CurrencyCode = dto.CurrencyCode,
                Status = dto.Status,
                Method = dto.Method,
                GatewayTransactionId = dto.GatewayTransactionId,
                CompletedAt = dto.CompletedAt,
                CreatedAt = dto.CreatedAt
            };

        private static List<FilterOption> BuildStatusOptions(string? current)
        {
            string[] statuses = ["Pending", "Completed", "Failed",
                                 "Refunded", "PartiallyRefunded"];
            return statuses.Select(s => new FilterOption
            {
                Value = s,
                Label = s,
                Selected = s == current
            }).ToList();
        }

        private static List<FilterOption> BuildMethodOptions(string? current)
        {
            string[] methods = ["Card", "UPI", "NetBanking", "Wallet", "COD"];
            return methods.Select(m => new FilterOption
            {
                Value = m,
                Label = m,
                Selected = m == current
            }).ToList();
        }
    }
}
