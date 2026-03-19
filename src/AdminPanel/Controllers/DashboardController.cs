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
        private readonly IProductApiClient _products;
        private readonly AuthTokenService _tokenService;

        public DashboardController(IProductApiClient products, AuthTokenService tokenService)
        {
            _products = products;
            _tokenService = tokenService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var token = _tokenService.GetAccessToken();
            if (token is null) return RedirectToAction("Login", "Auth");

            var pendingTask = _products.GetProductsAsync(
                token, page: 1, pageSize: 1,
                status: "PendingApproval");

            var activeTask = _products.GetProductsAsync(
                token, page: 1, pageSize: 1,
                status: "Active");

            await Task.WhenAll(pendingTask, activeTask);

            var vm = new DashboardViewModel
            {
                PendingApprovals = (await pendingTask)?.Data?.TotalCount ?? 0,
                ActiveProducts = (await activeTask)?.Data?.TotalCount ?? 0,
                // Stubs until Stats endpoint is built
                TotalRevenue = 0,
                TotalOrders = 0,
                ActiveSellers = 0
            };

            return View(vm);
        }

    }
}
