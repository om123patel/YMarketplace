 using AdminPanel.Dtos;
using AdminPanel.Dtos.Attributes;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Attributes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AttributesController : Controller
    {
        private readonly IApiClient _api;
        private readonly AuthTokenService _tokens;

        public AttributesController(IApiClient api, AuthTokenService tokens) { _api = api; _tokens = tokens; }

        public async Task<IActionResult> Index(string? search, string? status, string sortBy = "name", string sortDirection = "asc", CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.GetAttributeTemplatesAsync(token);
            var q = (result?.Data ?? []).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(a => a.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || a.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (status == "active") q = q.Where(a => a.IsActive);
            if (status == "inactive") q = q.Where(a => !a.IsActive);

            var items = q.Select(a => new AttributeTemplateListItem
            {
                Id = a.Id,
                Name = a.Name,
                CategoryId = a.CategoryId,
                CategoryName = a.CategoryName,
                IsActive = a.IsActive,
                ItemCount = a.Items.Count,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList();

            items = (sortBy, sortDirection) switch
            {
                ("name", "desc") => items.OrderByDescending(i => i.Name).ToList(),
                ("category", "asc") => items.OrderBy(i => i.CategoryName).ToList(),
                ("category", "desc") => items.OrderByDescending(i => i.CategoryName).ToList(),
                ("attributes", "asc") => items.OrderBy(i => i.ItemCount).ToList(),
                ("attributes", "desc") => items.OrderByDescending(i => i.ItemCount).ToList(),
                ("updated", "asc") => items.OrderBy(i => i.UpdatedAt).ToList(),
                ("updated", "desc") => items.OrderByDescending(i => i.UpdatedAt).ToList(),
                _ => items.OrderBy(i => i.Name).ToList()
            };

            return View(new AttributeListViewModel { Items = items, Search = search, StatusFilter = status, SortBy = sortBy, SortDirection = sortDirection });
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new CreateAttributeTemplateViewModel();
            await LoadCategories(vm.Categories);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAttributeTemplateViewModel vm, CancellationToken ct)
        {
            vm.Items = vm.Items.Where(r => !string.IsNullOrWhiteSpace(r.AttributeName)).ToList();
            if (!ModelState.IsValid) { await LoadCategories(vm.Categories); return View(vm); }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.CreateAttributeTemplateAsync(token, new CreateAttributeTemplateRequest
            {
                CategoryId = vm.CategoryId,
                Name = vm.Name,
                Items = vm.Items.Select((r, i) => new AttributeItemRequest
                {
                    AttributeName = r.AttributeName,
                    InputType = r.InputType,
                    IsRequired = r.IsRequired,
                    SortOrder = r.SortOrder > 0 ? r.SortOrder : i,
                    Options = ParseOptions(r.OptionsRaw)
                }).ToList()
            });

            if (result?.Success != true) { ModelState.AddModelError("", result?.Error ?? "Failed to create template."); await LoadCategories(vm.Categories); return View(vm); }
            TempData["Success"] = $"Template \"{vm.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet, Route("Attributes/{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.GetAttributeTemplateByIdAsync(token, id);
            if (result?.Data is null) { TempData["Error"] = "Template not found."; return RedirectToAction(nameof(Index)); }
            var a = result.Data;
            var vm = new EditAttributeTemplateViewModel
            {
                Id = a.Id,
                Name = a.Name,
                CategoryId = a.CategoryId,
                IsActive = a.IsActive,
                OriginalName = a.Name,
                CategoryName = a.CategoryName,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                Items = a.Items.Select(i => new AttributeItemFormRow
                {
                    AttributeName = i.AttributeName,
                    InputType = i.InputType,
                    IsRequired = i.IsRequired,
                    SortOrder = i.SortOrder,
                    OptionsRaw = i.ParsedOptions.Count > 0 ? string.Join(", ", i.ParsedOptions) : null
                }).ToList()
            };
            await LoadCategories(vm.Categories);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken, Route("Attributes/{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id, EditAttributeTemplateViewModel vm, CancellationToken ct)
        {
            vm.Items = vm.Items.Where(r => !string.IsNullOrWhiteSpace(r.AttributeName)).ToList();
            if (!ModelState.IsValid) { await LoadCategories(vm.Categories); return View(vm); }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.UpdateAttributeTemplateAsync(token, id, new UpdateAttributeTemplateRequest
            {
                Name = vm.Name,
                IsActive = vm.IsActive,
                Items = vm.Items.Select((r, i) => new AttributeItemRequest
                {
                    AttributeName = r.AttributeName,
                    InputType = r.InputType,
                    IsRequired = r.IsRequired,
                    SortOrder = r.SortOrder > 0 ? r.SortOrder : i,
                    Options = ParseOptions(r.OptionsRaw)
                }).ToList()
            });

            if (result?.Success != true) { ModelState.AddModelError("", result?.Error ?? "Failed to update template."); await LoadCategories(vm.Categories); return View(vm); }
            TempData["Success"] = $"Template \"{vm.Name}\" updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Route("Attributes/{id:int}/Delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.DeleteAttributeTemplateAsync(token, id);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Template deleted." : result?.Error ?? "Failed to delete template.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet, Route("Attributes/RowTemplate")]
        public IActionResult RowTemplate(int index)
            => PartialView("_AttributeRow", new AttributeRowViewModel { Row = new AttributeItemFormRow(), Index = index });

        private async Task LoadCategories(List<CategoryOption> list)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.GetCategoriesAsync(token);
            list.AddRange(result?.Data?.Select(c => new CategoryOption { Id = c.Id, Name = c.Name }) ?? []);
        }

        private static List<string> ParseOptions(string? raw)
            => string.IsNullOrWhiteSpace(raw) ? []
             : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}