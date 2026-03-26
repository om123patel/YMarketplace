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
            int page = 1, int pageSize = 10,
            CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _categories.GetPaged(token,
                page, pageSize, search, status, sortBy, sortDirection);

            var data = result?.Data;
            List<CategoryItem> items;

            if (data?.Items?.Any() == true)
            {
                // Build a lookup of parentId -> child count to avoid O(n^2) counting.
                var parentCounts = data.Items
                    .Where(x => x.ParentId.HasValue)
                    .GroupBy(x => x.ParentId!.Value)
                    .ToDictionary(g => g.Key, g => g.Count());

                items = data.Items.Select(c => new CategoryItem
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
                    ChildCount = parentCounts.TryGetValue(c.Id, out var cc) ? cc : 0,
                    Depth = c.ParentId.HasValue ? 1 : 0
                }).ToList();
            }
            else
            {
                items = new List<CategoryItem>();
            }

            var vm = new CategoryListViewModel
            {
                Items = items,
                TotalCount = data?.TotalCount ?? 0,
                Page = data?.Page ?? page,
                PageSize = data?.PageSize ?? pageSize,
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
/* Pseudocode / Plan (detailed):
   1. Retrieve access token (fall back to empty string).
   2. Call the paging API to get categories (preserve original call signature).
   3. Cache the returned Data in a local variable to avoid repeated null-conditional access.
   4. If there are no items, prepare an empty list for the view model.
   5. If items exist:
      a. Build a dictionary that maps parentId -> number of children by grouping Items on ParentId.
         - Filter out null ParentId values before grouping.
         - This makes child-count lookup O(1) per item instead of O(n) per item.
      b. Project each item to CategoryItem:
         - Copy scalar properties directly.
         - Determine ChildCount by looking up in the parentCounts dictionary.
         - Determine Depth the same as original (1 if ParentId.HasValue else 0).
      c. Materialize projection to a List once.
   6. Build the CategoryListViewModel using data from the API response (or fallbacks).
   7. Call vm.BuildRouteData() and return the view.
   8. Keep cancellation token usage and API signature unchanged to avoid breaking interface contracts.
*/