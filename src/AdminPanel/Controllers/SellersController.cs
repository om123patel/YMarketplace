using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Seller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class SellersController : Controller
    {
        private readonly ISellerApiClient _sellers;
        private readonly AuthTokenService _tokens;
        public SellersController(ISellerApiClient sellers, AuthTokenService tokens)
        { _sellers = sellers; _tokens = tokens; }

        // ══════════════════════════════════════════════════════
        // GET /Sellers
        // ══════════════════════════════════════════════════════
        public async Task<IActionResult> Index(
      string? search,
      string? sellerStatus,
      string sortBy = "createdAt",
      string sortDirection = "desc",
      int page = 1,
      int pageSize = 10,
      CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? string.Empty;

            // Fix: correct status value
            var pendingStatus = "Pending";

            var listTask = _sellers.GetPaged(
                token,
                page,
                pageSize,
                search,
                sellerStatus,
                sortBy,
                sortDirection);

            var pendingTask = _sellers.GetPaged(
                token,
                1,              // always first page
                1,              // only need count
                null,
                pendingStatus,
                null,
                null);

            await Task.WhenAll(listTask, pendingTask);

            var listResult = await listTask;
            var pendingResult = await pendingTask;

            var items = listResult?.Data?.Items?
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
                })
                .ToList() ?? new List<SellerListItem>();

            var vm = new SellerListViewModel
            {
                Items = items,
                Page = listResult?.Data?.Page ?? page,
                PageSize = listResult?.Data?.PageSize ?? pageSize,
                TotalCount = listResult?.Data?.TotalCount ?? 0,
                Search = search,
                SellerStatus = sellerStatus,
                SortBy = sortBy,
                SortDirection = sortDirection,
                PendingCount = pendingResult?.Data?.TotalCount ?? 0
            };

            vm.BuildRouteData(new Dictionary<string, string?>
            {
                ["sellerStatus"] = sellerStatus,
                ["search"] = search
            });

            return View(vm);
        }

        [HttpGet]
        [Route("Sellers/{id:guid}")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _sellers.GetSellerByIdAsync(token, id);

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
            var result = await _sellers.ApproveSellerAsync(token, id);

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
            var result = await _sellers.RejectSellerAsync(token, id, reason);

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
            var result = await _sellers.SuspendSellerAsync(token, id, reason);

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
            var result = await _sellers.ActivateSellerAsync(token, id);

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
            return RedirectToAction(nameof(Index), new { id });
        }
    }
}