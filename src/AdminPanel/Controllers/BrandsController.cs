using AdminPanel.Dtos;
using AdminPanel.Dtos.Brands;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Brands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class BrandsController : Controller
    {
        private readonly IApiClient _api;
        private readonly AuthTokenService _tokens;

        public BrandsController(IApiClient api, AuthTokenService tokens) { _api = api; _tokens = tokens; }

        public async Task<IActionResult> Index(string? search, string? status, string sortBy = "name", string sortDirection = "asc", CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.GetBrandsAsync(token);
            var q = (result?.Data ?? []).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(b => b.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || b.Slug.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (status == "active") q = q.Where(b => b.IsActive);
            if (status == "inactive") q = q.Where(b => !b.IsActive);

            var items = q.Select(b => new BrandItem
            {
                Id = b.Id,
                Name = b.Name,
                Slug = b.Slug,
                Description = b.Description,
                LogoUrl = b.LogoUrl,
                WebsiteUrl = b.WebsiteUrl,
                IsActive = b.IsActive,
                ProductCount = b.ProductCount,
                CreatedAt = b.CreatedAt
            }).ToList();

            items = (sortBy, sortDirection) switch
            {
                ("name", "desc") => items.OrderByDescending(i => i.Name).ToList(),
                ("products", "asc") => items.OrderBy(i => i.ProductCount).ToList(),
                ("products", "desc") => items.OrderByDescending(i => i.ProductCount).ToList(),
                ("created", "asc") => items.OrderBy(i => i.CreatedAt).ToList(),
                ("created", "desc") => items.OrderByDescending(i => i.CreatedAt).ToList(),
                _ => items.OrderBy(i => i.Name).ToList()
            };

            return View(new BrandListViewModel { Items = items, Search = search, StatusFilter = status, SortBy = sortBy, SortDirection = sortDirection });
        }

        [HttpGet]
        public IActionResult Create() => View(new CreateBrandViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBrandViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.CreateBrandAsync(token, new CreateBrandRequest { Name = vm.Name, Slug = vm.Slug, Description = vm.Description, Website = vm.WebsiteUrl });
            if (result?.Success != true) { ModelState.AddModelError("", result?.Error ?? "Failed to create brand."); return View(vm); }
            TempData["Success"] = $"Brand \"{vm.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet, Route("Brands/{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.GetBrandByIdAsync(token, id);
            if (result?.Data is null) { TempData["Error"] = "Brand not found."; return RedirectToAction(nameof(Index)); }
            var b = result.Data;
            return View(new EditBrandViewModel { Id = b.Id, Name = b.Name, Slug = b.Slug, Description = b.Description, LogoUrl = b.LogoUrl, WebsiteUrl = b.WebsiteUrl, IsActive = b.IsActive, OriginalName = b.Name, ProductCount = b.ProductCount, CreatedAt = b.CreatedAt });
        }

        [HttpPost, ValidateAntiForgeryToken, Route("Brands/{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id, EditBrandViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.UpdateBrandAsync(token, id, new UpdateBrandRequest { Name = vm.Name, Slug = vm.Slug, Description = vm.Description, LogoUrl = vm.LogoUrl, WebsiteUrl = vm.WebsiteUrl, IsActive = vm.IsActive });
            if (result?.Success != true) { ModelState.AddModelError("", result?.Error ?? "Failed to update brand."); return View(vm); }
            TempData["Success"] = $"Brand \"{vm.Name}\" updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Route("Brands/{id:int}/Toggle")]
        public async Task<IActionResult> Toggle(int id, bool activate, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.ToggleBrandActiveAsync(token, id, activate);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? (activate ? "Brand activated." : "Brand deactivated.") : result?.Error ?? "Failed.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Route("Brands/{id:int}/Delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.DeleteBrandAsync(token, id);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Brand deleted." : result?.Error ?? "Cannot delete — brand may have products.";
            return RedirectToAction(nameof(Index));
        }
    }
}