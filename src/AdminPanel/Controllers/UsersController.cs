using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : Controller
    {
        private readonly IUserApiClient _users;
        private readonly AuthTokenService _tokens;

        public UsersController(IUserApiClient api, AuthTokenService tokens)
        {
            _users = api;
            _tokens = tokens;
        }

        public async Task<IActionResult> Index(
            string? search, string? role, string? status,
            string sortBy = "createdat", string sortDirection = "desc",
            int page = 1, CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _users.GetUsersAsync(
                token, page, 20, search, role, status, sortBy, sortDirection);

            var vm = new UserListViewModel
            {
                Items = result?.Data?.Items.Select(u => new UserListItem
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    AvatarUrl = u.AvatarUrl,
                    Role = u.Role,
                    Status = u.Status,
                    LastLoginAt = u.LastLoginAt,
                    FailedLoginAttempts = u.FailedLoginAttempts,
                    CreatedAt = u.CreatedAt
                }).ToList() ?? [],
                Page = result?.Data?.Page ?? page,
                PageSize = result?.Data?.PageSize ?? 20,
                TotalCount = result?.Data?.TotalCount ?? 0,
                Search = search,
                StatusFilter = status,
                Role = role,
                SortBy = sortBy,
                SortDirection = sortDirection
            };
            vm.BuildRouteData(new() { ["role"] = role });
            return View(vm);
        }


        // ══════════════════════════════════════════════════════
        // GET /Users/{id}
        // ══════════════════════════════════════════════════════
        [HttpGet]
        [Route("Users/{id:guid}")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _users.GetUserByIdAsync(token, id);

            if (result?.Data is null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var u = result.Data;
            var vm = new UserDetailViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                AvatarUrl = u.AvatarUrl,
                Role = u.Role,
                Status = u.Status,
                LastLoginAt = u.LastLoginAt,
                FailedLoginAttempts = u.FailedLoginAttempts,
                LockedUntil = u.LockedUntil,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                // Admin cannot suspend themselves or other admins
                CanSuspend = u.Status == "Active" && u.Role != "Admin",
                CanActivate = u.Status == "Suspended" || u.Status == "Inactive",
                CanDelete = u.Role != "Admin"
            };

            return View(vm);
        }

        // ══════════════════════════════════════════════════════
        // POST /Users/{id}/Suspend
        // ══════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Users/{id:guid}/Suspend")]
        public async Task<IActionResult> Suspend(
            Guid id, string reason, string? returnUrl, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "A reason is required to suspend a user.";
                return RedirectBack(returnUrl, id);
            }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _users.SuspendUserAsync(token, id, reason);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "User suspended."
                    : result?.Error ?? "Failed to suspend user.";

            return RedirectBack(returnUrl, id);
        }

        // ══════════════════════════════════════════════════════
        // POST /Users/{id}/Activate
        // ══════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Users/{id:guid}/Activate")]
        public async Task<IActionResult> Activate(
            Guid id, string? returnUrl, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _users.ActivateUserAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "User activated."
                    : result?.Error ?? "Failed to activate user.";

            return RedirectBack(returnUrl, id);
        }

        // ══════════════════════════════════════════════════════
        // POST /Users/{id}/Delete
        // ══════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Users/{id:guid}/Delete")]
        public async Task<IActionResult> Delete(
            Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _users.DeleteUserAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "User deleted."
                    : result?.Error ?? "Failed to delete user.";

            return RedirectToAction(nameof(Index));
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