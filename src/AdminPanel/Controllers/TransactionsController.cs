using AdminPanel.Dtos.Payments;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Grid;
using AdminPanel.ViewModels.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("payments/transactions")]
    public class TransactionsController : Controller
    {
        private readonly ITransactionApiClient _client;
        private readonly AuthTokenService _tokens;

        public TransactionsController(
            ITransactionApiClient client, AuthTokenService tokens)
        {
            _client = client;
            _tokens = tokens;
        }

        // GET /payments/transactions
        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? status, string? method,
            string? dateFrom, string? dateTo,
            string? search, int page = 1,
            CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";

            var result = await _client.GetTransactionsAsync(
                token, page, 20, status, method, dateFrom, dateTo, search);

            var vm = new TransactionIndexViewModel
            {
                Items = result?.Data?.Items.Select(t => new TransactionListItemViewModel
                {
                    Id = t.Id,
                    OrderId = t.OrderId,
                    SellerId = t.SellerId,
                    Amount = t.Amount,
                    CommissionAmount = t.CommissionAmount,
                    SellerAmount = t.SellerAmount,
                    CurrencyCode = t.CurrencyCode,
                    Status = t.Status,
                    Method = t.Method,
                    GatewayTransactionId = t.GatewayTransactionId,
                    CompletedAt = t.CompletedAt,
                    CreatedAt = t.CreatedAt
                }).ToList() ?? [],
                Page = result?.Data?.Page ?? page,
                PageSize = result?.Data?.PageSize ?? 20,
                TotalCount = result?.Data?.TotalCount ?? 0,
                Search = search,
                SortBy = "createdat",
                SortDirection = "desc",
                StatusFilter = status,
                MethodFilter = method,
                DateFromFilter = dateFrom,
                DateToFilter = dateTo,
                StatusOptions = BuildOptions(
                    ["Pending", "Completed", "Failed", "Refunded", "PartiallyRefunded"],
                    status),
                MethodOptions = BuildOptions(
                    ["Card", "UPI", "NetBanking", "Wallet", "COD"],
                    method),
                RouteValues = new()
                {
                    ["status"] = status,
                    ["method"] = method,
                    ["dateFrom"] = dateFrom,
                    ["dateTo"] = dateTo,
                    ["search"] = search
                }
            };

            vm.BuildRouteData(new() { ["status"] = status, ["search"] = search });
            return View(vm);
        }

        // GET /payments/transactions/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.GetByIdAsync(token, id);

            if (result?.Data is null)
            {
                TempData["Error"] = "Transaction not found.";
                return RedirectToAction(nameof(Index));
            }

            var t = result.Data;
            var vm = new TransactionDetailViewModel
            {
                Id = t.Id,
                OrderId = t.OrderId,
                BuyerId = t.BuyerId,
                SellerId = t.SellerId,
                StoreId = t.StoreId,
                Amount = t.Amount,
                CommissionAmount = t.CommissionAmount,
                SellerAmount = t.SellerAmount,
                CurrencyCode = t.CurrencyCode,
                Status = t.Status,
                Method = t.Method,
                GatewayTransactionId = t.GatewayTransactionId,
                GatewayProvider = t.GatewayProvider,
                RefundedAmount = t.RefundedAmount,
                RefundReason = t.RefundReason,
                RefundedAt = t.RefundedAt,
                CompletedAt = t.CompletedAt,
                FailedAt = t.FailedAt,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            };

            return View(vm);
        }

        // POST /payments/transactions/{id}/refund
        [HttpPost("{id:guid}/refund")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(
            Guid id, decimal amount, string reason, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.RefundAsync(token, id,
                new RefundTransactionRequest { Amount = amount, Reason = reason });

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Refund processed successfully."
                    : result?.Error ?? "Failed to process refund.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        // ── Helpers ────────────────────────────────────────────────
        private static List<FilterOption> BuildOptions(
            string[] values, string? current)
            => values.Select(v => new FilterOption(v, v)).ToList();
    }
}