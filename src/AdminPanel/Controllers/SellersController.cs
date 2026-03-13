using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class SellersController : Controller
    {
        private readonly IApiClient _api;
        private readonly AuthTokenService _tokens;

        public SellersController(IApiClient api, AuthTokenService tokens)
        {
            _api = api;
            _tokens = tokens;
        }

        // ══════════════════════════════════════════════════════
        // GET /Sellers
        // ══════════════════════════════════════════════════════
        public async Task<IActionResult> Index(
            string? search,
            string? sellerStatus,
            string sortBy = "createdat",
            string sortDirection = "desc",
            int page = 1,
            CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";

            // Fetch list + pending count in parallel
            var listTask = _api.GetSellersAsync(token, page, 20,
                                search, sellerStatus, sortBy, sortDirection);
            var pendingTask = _api.GetSellersAsync(token, 1, 1,
                                null, "PendingApproval");

            await Task.WhenAll(listTask, pendingTask);

            var result = await listTask;
            var vm = new SellerListViewModel
            {
                Items = result?.Data?.Items
                    .Select(s => new SellerListItem
                    {
                        Id = s.Id,
                        UserId = s.UserId,
                        FullName = s.FullName,
                        Email = s.Email,
                        AvatarUrl = s.AvatarUrl,
                        BusinessName = s.BusinessName,
                        City = s.City,
                        Country = s.Country,
                        SellerStatus = s.SellerStatus,
                        TotalProducts = s.TotalProducts,
                        TotalOrders = s.TotalOrders,
                        TotalRevenue = s.TotalRevenue,
                        Rating = s.Rating,
                        CreatedAt = s.CreatedAt
                    }).ToList() ?? [],
                Page = result?.Data?.Page ?? page,
                PageSize = result?.Data?.PageSize ?? 20,
                TotalCount = result?.Data?.TotalCount ?? 0,
                Search = search,
                SellerStatus = sellerStatus,
                SortBy = sortBy,
                SortDirection = sortDirection,
                PendingCount = (await pendingTask)?.Data?.TotalCount ?? 0
            };

            return View(vm);
        }

        // ══════════════════════════════════════════════════════
        // GET /Sellers/{id}
        // ══════════════════════════════════════════════════════
        [HttpGet]
        [Route("Sellers/{id:guid}")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.GetSellerByIdAsync(token, id);

            if (result?.Data is null)
            {
                TempData["Error"] = "Seller not found.";
                return RedirectToAction(nameof(Index));
            }

            var s = result.Data;
            var vm = new SellerDetailViewModel
            {
                Id = s.Id,
                UserId = s.UserId,
                FullName = s.FullName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                AvatarUrl = s.AvatarUrl,
                UserStatus = s.UserStatus,
                BusinessName = s.BusinessName,
                BusinessEmail = s.BusinessEmail,
                BusinessPhone = s.BusinessPhone,
                Description = s.Description,
                LogoUrl = s.LogoUrl,
                WebsiteUrl = s.WebsiteUrl,
                City = s.City,
                Country = s.Country,
                SellerStatus = s.SellerStatus,
                RejectionReason = s.RejectionReason,
                TotalProducts = s.TotalProducts,
                TotalOrders = s.TotalOrders,
                TotalRevenue = s.TotalRevenue,
                Rating = s.Rating,
                CreatedAt = s.CreatedAt,
                ApprovedAt = s.ApprovedAt,
                CanApprove = s.SellerStatus == "PendingApproval",
                CanReject = s.SellerStatus == "PendingApproval",
                CanSuspend = s.SellerStatus == "Active",
                CanActivate = s.SellerStatus == "Suspended"
            };

            return View(vm);
        }

        // ══════════════════════════════════════════════════════
        // POST /Sellers/{id}/Approve
        // ══════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Sellers/{id:guid}/Approve")]
        public async Task<IActionResult> Approve(
            Guid id, string? returnUrl, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.ApproveSellerAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Seller approved. They can now list products."
                    : result?.Error ?? "Failed to approve seller.";

            return RedirectBack(returnUrl, id);
        }

        // ══════════════════════════════════════════════════════
        // POST /Sellers/{id}/Reject
        // ══════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Sellers/{id:guid}/Reject")]
        public async Task<IActionResult> Reject(
            Guid id, string reason, string? returnUrl, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "A rejection reason is required.";
                return RedirectBack(returnUrl, id);
            }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.RejectSellerAsync(token, id, reason);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Seller application rejected."
                    : result?.Error ?? "Failed to reject seller.";

            return RedirectBack(returnUrl, id);
        }

        // ══════════════════════════════════════════════════════
        // POST /Sellers/{id}/Suspend
        // ══════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Sellers/{id:guid}/Suspend")]
        public async Task<IActionResult> Suspend(
            Guid id, string reason, string? returnUrl, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "A reason is required to suspend a seller.";
                return RedirectBack(returnUrl, id);
            }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.SuspendSellerAsync(token, id, reason);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Seller suspended."
                    : result?.Error ?? "Failed to suspend seller.";

            return RedirectBack(returnUrl, id);
        }

        // ══════════════════════════════════════════════════════
        // POST /Sellers/{id}/Activate
        // ══════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Sellers/{id:guid}/Activate")]
        public async Task<IActionResult> Activate(
            Guid id, string? returnUrl, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.ActivateSellerAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Seller reactivated."
                    : result?.Error ?? "Failed to activate seller.";

            return RedirectBack(returnUrl, id);
        }

        // ── Helpers ──────────────────────────────────────────
        private IActionResult RedirectBack(string? returnUrl, Guid id)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}