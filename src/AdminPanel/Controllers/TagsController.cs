using AdminPanel.Dtos.Tags;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Tags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class TagsController : Controller
    {
        private readonly IApiClient _api;
        private readonly AuthTokenService _tokens;

        public TagsController(IApiClient api, AuthTokenService tokens) { _api = api; _tokens = tokens; }

        public async Task<IActionResult> Index(string? search, string sortBy = "name", string sortDirection = "asc", CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.GetTagsAsync(token);
            var q = (result?.Data ?? []).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(t => t.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || t.Slug.Contains(search, StringComparison.OrdinalIgnoreCase));

            var items = q.Select(t => new TagItem
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                ProductCount = t.ProductCount,
                CreatedAt = t.CreatedAt
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

            return View(new TagListViewModel { Items = items, Search = search, SortBy = sortBy, SortDirection = sortDirection });
        }

        [HttpGet]
        public IActionResult Create() => View(new CreateTagViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTagViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.CreateTagAsync(token, new CreateTagRequest { Name = vm.Name, Slug = vm.Slug });
            if (result?.Success != true) { ModelState.AddModelError("", result?.Error ?? "Failed to create tag."); return View(vm); }
            TempData["Success"] = $"Tag \"{vm.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet, Route("Tags/{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.GetTagByIdAsync(token, id);
            if (result?.Data is null) { TempData["Error"] = "Tag not found."; return RedirectToAction(nameof(Index)); }
            var t = result.Data;
            return View(new EditTagViewModel { Id = t.Id, Name = t.Name, Slug = t.Slug, OriginalName = t.Name, ProductCount = t.ProductCount, CreatedAt = t.CreatedAt });
        }

        [HttpPost, ValidateAntiForgeryToken, Route("Tags/{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id, EditTagViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.UpdateTagAsync(token, id, new UpdateTagRequest { Name = vm.Name, Slug = vm.Slug });
            if (result?.Success != true) { ModelState.AddModelError("", result?.Error ?? "Failed to update tag."); return View(vm); }
            TempData["Success"] = $"Tag \"{vm.Name}\" updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Route("Tags/{id:int}/Delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.DeleteTagAsync(token, id);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Tag deleted." : result?.Error ?? "Cannot delete — tag may be in use.";
            return RedirectToAction(nameof(Index));
        }
    }
}