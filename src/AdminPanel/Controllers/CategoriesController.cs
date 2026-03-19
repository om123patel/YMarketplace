using AdminPanel.Dtos;
using AdminPanel.Dtos.Categories;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryApiClient _categories;
        private readonly AuthTokenService _tokens;
        public CategoriesController(ICategoryApiClient categories, AuthTokenService tokens)
        { _categories = categories; _tokens = tokens; }

        // GET /Categories
        public async Task<IActionResult> Index(
            string? search, string? status,
            string sortBy = "name", string sortDirection = "asc",
            CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _categories.GetCategoriesAsync(token);
            var all = result?.Data ?? [];

            var q = all.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || c.Slug.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (status == "active") q = q.Where(c => c.IsActive);
            if (status == "inactive") q = q.Where(c => !c.IsActive);

            var items = q.Select(c => new CategoryItem
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ParentId = c.ParentId,
                ParentName = c.ParentName,
                IsActive = c.IsActive,
                SortOrder = c.SortOrder,
                ProductCount = c.ProductCount,
                ChildCount = all.Count(x => x.ParentId == c.Id),
                Depth = c.ParentId.HasValue ? 1 : 0
            }).ToList();

            items = (sortBy, sortDirection) switch
            {
                ("name", "desc") => items.OrderByDescending(i => i.Name).ToList(),
                ("products", "asc") => items.OrderBy(i => i.ProductCount).ToList(),
                ("products", "desc") => items.OrderByDescending(i => i.ProductCount).ToList(),
                ("sort", "asc") => items.OrderBy(i => i.SortOrder).ToList(),
                ("sort", "desc") => items.OrderByDescending(i => i.SortOrder).ToList(),
                _ => items.OrderBy(i => i.Name).ToList()
            };

            var vm = new CategoryListViewModel
            {
                Items = items,
                TotalCount = items.Count,
                Page = 1,
                PageSize = items.Count == 0 ? 20 : items.Count,
                Search = search,
                StatusFilter = status,
                SortBy = sortBy,
                SortDirection = sortDirection
            };
            vm.BuildRouteData();
            return View(vm);
        }


        // GET /Categories/Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new CreateCategoryViewModel();
            await PopulateParents(vm.ParentOptions, _tokens.GetAccessToken() ?? "");
            return View(vm);
        }

        // POST /Categories/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateParents(vm.ParentOptions, _tokens.GetAccessToken() ?? "");
                return View(vm);
            }
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _categories.CreateCategoryAsync(token, new CreateCategoryRequest
            {
                Name = vm.Name,
                Slug = vm.Slug,
                Description = vm.Description,
                ImageUrl = vm.ImageUrl,
                IconUrl = vm.IconUrl,
                ParentId = vm.ParentId,
                SortOrder = vm.SortOrder
            });
            if (result?.Success != true)
            {
                ModelState.AddModelError("", result?.Error ?? "Failed to create category.");
                await PopulateParents(vm.ParentOptions, token);
                return View(vm);
            }
            TempData["Success"] = $"Category \"{vm.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Categories/5/Edit
        [HttpGet, Route("Categories/{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _categories.GetCategoryByIdAsync(token, id);
            if (result?.Data is null) { TempData["Error"] = "Category not found."; return RedirectToAction(nameof(Index)); }
            var c = result.Data;
            var vm = new EditCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ParentId = c.ParentId,
                IsActive = c.IsActive,
                SortOrder = c.SortOrder,
                ProductCount = c.ProductCount,
                OriginalName = c.Name
            };
            await PopulateParents(vm.ParentOptions, token, excludeId: id);
            return View(vm);
        }

        // POST /Categories/5/Edit
        [HttpPost, ValidateAntiForgeryToken, Route("Categories/{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id, EditCategoryViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateParents(vm.ParentOptions, _tokens.GetAccessToken() ?? "", excludeId: id);
                return View(vm);
            }
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _categories.UpdateCategoryAsync(token, id, new UpdateCategoryRequest
            {
                Name = vm.Name,
                Slug = vm.Slug,
                Description = vm.Description,
                ImageUrl = vm.ImageUrl,
                IconUrl = vm.IconUrl,
                ParentId = vm.ParentId,
                SortOrder = vm.SortOrder,
                IsActive = vm.IsActive
            });
            if (result?.Success != true)
            {
                ModelState.AddModelError("", result?.Error ?? "Failed to update category.");
                await PopulateParents(vm.ParentOptions, token, excludeId: id);
                return View(vm);
            }
            TempData["Success"] = $"Category \"{vm.Name}\" updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Categories/5/Delete
        [HttpPost, ValidateAntiForgeryToken, Route("Categories/{id:int}/Delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _categories.DeleteCategoryAsync(token, id);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Category deleted." : result?.Error ?? "Cannot delete this category.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Categories/5/Toggle
        [HttpPost, ValidateAntiForgeryToken, Route("Categories/{id:int}/Toggle")]
        public async Task<IActionResult> Toggle(int id, bool activate, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _categories.ToggleCategoryActiveAsync(token, id, activate);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? (activate ? "Category activated." : "Category deactivated.")
                    : result?.Error ?? "Failed to update status.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateParents(List<ParentCategoryOption> list, string token, int? excludeId = null)
        {
            var result = await _categories.GetCategoriesAsync(token);
            list.AddRange(result?.Data?
                .Where(c => c.ParentId == null && c.Id != excludeId)
                .Select(c => new ParentCategoryOption { Id = c.Id, Name = c.Name }) ?? []);
        }
    }
}