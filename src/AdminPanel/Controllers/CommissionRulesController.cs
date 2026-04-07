using AdminPanel.Dtos.Payments;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("payments/commission-rules")]
    public class CommissionRulesController : Controller
    {
        private readonly ICommissionRuleApiClient _client;
        private readonly AuthTokenService _tokens;

        public CommissionRulesController(
            ICommissionRuleApiClient client, AuthTokenService tokens)
        {
            _client = client;
            _tokens = tokens;
        }

        // GET /payments/commission-rules
        [HttpGet("")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.GetAllAsync(token);

            var vm = new CommissionRuleIndexViewModel
            {
                Rules = (result?.Data ?? []).Select(r => new CommissionRuleRowViewModel
                {
                    Id = r.Id,
                    CategoryId = r.CategoryId,
                    CategoryName = r.CategoryName,
                    Name = r.Name,
                    RatePercent = r.RatePercent,
                    IsActive = r.IsActive,
                    UpdatedAt = r.UpdatedAt,
                    EditName = r.Name,
                    EditCategoryId = r.CategoryId,
                    EditRatePercent = r.RatePercent
                }).ToList()
            };

            return View(vm);
        }

        // POST /payments/commission-rules
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CommissionRuleIndexViewModel vm, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(vm.NewName))
            {
                TempData["Error"] = "Rule name is required.";
                return RedirectToAction(nameof(Index));
            }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.CreateAsync(token, new CreateCommissionRuleRequest
            {
                Name = vm.NewName,
                CategoryId = vm.NewCategoryId,
                RatePercent = vm.NewRatePercent
            });

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? $"Commission rule '{vm.NewName}' created."
                    : result?.Error ?? "Failed to create commission rule.";

            return RedirectToAction(nameof(Index));
        }

        // POST /payments/commission-rules/{id}/edit
        [HttpPost("{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, string name, int? categoryId, decimal ratePercent,
            CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.UpdateAsync(token, id,
                new UpdateCommissionRuleRequest
                {
                    Name = name,
                    CategoryId = categoryId,
                    RatePercent = ratePercent
                });

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Commission rule updated."
                    : result?.Error ?? "Failed to update rule.";

            return RedirectToAction(nameof(Index));
        }

        // POST /payments/commission-rules/{id}/activate
        [HttpPost("{id:int}/activate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.ActivateAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Rule activated." : result?.Error ?? "Failed.";

            return RedirectToAction(nameof(Index));
        }

        // POST /payments/commission-rules/{id}/deactivate
        [HttpPost("{id:int}/deactivate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.DeactivateAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Rule deactivated." : result?.Error ?? "Failed.";

            return RedirectToAction(nameof(Index));
        }

        // POST /payments/commission-rules/{id}/delete
        [HttpPost("{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _client.DeleteAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Rule deleted." : result?.Error ?? "Failed.";

            return RedirectToAction(nameof(Index));
        }
    }
}