using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels;
using AdminPanel.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class DashboardController : Controller
    {
        private readonly IApiClient _api;
        private readonly AuthTokenService _tokenService;

        public DashboardController(IApiClient api, AuthTokenService tokenService)
        {
            _api = api;
            _tokenService = tokenService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var token = _tokenService.GetAccessToken();
            if (token is null) return RedirectToAction("Login", "Auth");

            // Get pending approvals count from products
            var products = await _api.GetProductsAsync(
                token, page: 1, pageSize: 1,
                search: null, status: "PendingApproval", categoryId: null);

            var vm = new DashboardViewModel
            {
                // Real data from API — these numbers would come from
                // a dedicated dashboard/stats endpoint you'd add later
                PendingApprovals = products?.Data?.TotalCount ?? 0,

                // Placeholder stats until dedicated stats endpoint is added
                TotalRevenue = 482000,
                TotalOrders = 1284,
                ActiveProducts = products?.Data?.TotalCount ?? 0,
                ActiveSellers = 218,
            };

            return View(vm);
        }
    }
}
