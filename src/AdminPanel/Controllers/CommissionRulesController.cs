using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.DTOs.CommissionRules;

namespace AdminPanel.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("payments/commission-rules")]
    public class CommissionRulesController : Controller
    {
        private readonly ICommissionRuleApiClient _client;

        public CommissionRulesController(ICommissionRuleApiClient client)
            => _client = client;

        // GET /payments/commission-rules
        public async Task<IActionResult> Index()
        {
            var rules = await _client.GetAllAsync() ?? [];

            var vm = new CommissionRuleIndexViewModel
            {
                Rules = rules.Select(r => new CommissionRuleRowViewModel
                {
                    Id = r.Id,
                    CategoryId = r.CategoryId,
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CommissionRuleIndexViewModel vm)
        {
            var result = await _client.CreateAsync(new CreateCommissionRuleDto
            {
                Name = vm.NewName ?? string.Empty,
                CategoryId = vm.NewCategoryId,
                RatePercent = vm.NewRatePercent
            });

            TempData[result is not null ? "SuccessMessage" : "ErrorMessage"] =
                result is not null
                    ? $"Commission rule '{result.Name}' created."
                    : "Failed to create commission rule.";

            return RedirectToAction(nameof(Index));
        }

        // POST /payments/commission-rules/{id}/edit
        [HttpPost("{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, string name, int? categoryId, decimal ratePercent)
        {
            var result = await _client.UpdateAsync(id, new UpdateCommissionRuleDto
            {
                Name = name,
                CategoryId = categoryId,
                RatePercent = ratePercent
            });

            TempData[result is not null ? "SuccessMessage" : "ErrorMessage"] =
                result is not null
                    ? "Commission rule updated."
                    : "Failed to update rule.";

            return RedirectToAction(nameof(Index));
        }

        // POST /payments/commission-rules/{id}/activate
        [HttpPost("{id:int}/activate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var ok = await _client.ActivateAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                ok ? "Rule activated." : "Failed to activate rule.";

            return RedirectToAction(nameof(Index));
        }

        // POST /payments/commission-rules/{id}/deactivate
        [HttpPost("{id:int}/deactivate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var ok = await _client.DeactivateAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                ok ? "Rule deactivated." : "Failed to deactivate rule.";

            return RedirectToAction(nameof(Index));
        }

        // POST /payments/commission-rules/{id}/delete
        [HttpPost("{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _client.DeleteAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                ok ? "Rule deleted." : "Failed to delete rule.";

            return RedirectToAction(nameof(Index));
        }
    }
}
