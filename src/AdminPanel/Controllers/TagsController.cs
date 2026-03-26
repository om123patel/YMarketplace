using AdminPanel.Dtos.Tags;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Brands;
using AdminPanel.ViewModels.Tags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class TagsController : Controller
    {
        private readonly ITagApiClient _tags;
        private readonly AuthTokenService _tokens;
        public TagsController(ITagApiClient tags, AuthTokenService tokens)
        { _tags = tags; _tokens = tokens; }

        public async Task<IActionResult> Index(
             string? search,
             string sortBy = "name", string sortDirection = "asc",
              int page = 1, int pageSize = 10,
             CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";

            var result = await _tags.GetPaged(token,
                page, pageSize, search, string.Empty, sortBy, sortDirection);

            var data = result?.Data;
            List<TagItem> items;
            if (data?.Items?.Any() == true)
            {
                items = data.Items.Select(t => new TagItem
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    ProductCount = t.ProductCount,
                    CreatedAt = t.CreatedAt
                }).ToList();
            }
            else
            {
                items = new List<TagItem>();
            }


            

            var vm = new TagListViewModel
            {
                Items = items,
                TotalCount = data?.TotalCount ?? 0,
                Page = data?.Page ?? page,
                PageSize = data?.PageSize ?? pageSize,
                Search = search,
                StatusFilter = string.Empty,
                SortBy = sortBy,
                SortDirection = sortDirection
            };
            vm.BuildRouteData();
            return View(vm);
        }


        [HttpGet]
        public IActionResult Create() => View(new CreateTagViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTagViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _tags.CreateTagAsync(token, new CreateTagRequest { Name = vm.Name, Slug = vm.Slug });
            if (result?.Success != true) { ModelState.AddModelError("", result?.Error ?? "Failed to create tag."); return View(vm); }
            TempData["Success"] = $"Tag \"{vm.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet, Route("Tags/{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _tags.GetTagByIdAsync(token, id);
            if (result?.Data is null) { TempData["Error"] = "Tag not found."; return RedirectToAction(nameof(Index)); }
            var t = result.Data;
            return View(new EditTagViewModel { Id = t.Id, Name = t.Name, Slug = t.Slug, OriginalName = t.Name, ProductCount = t.ProductCount, CreatedAt = t.CreatedAt });
        }

        [HttpPost, ValidateAntiForgeryToken, Route("Tags/{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id, EditTagViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _tags.UpdateTagAsync(token, id, new UpdateTagRequest { Name = vm.Name, Slug = vm.Slug });
            if (result?.Success != true) { ModelState.AddModelError("", result?.Error ?? "Failed to update tag."); return View(vm); }
            TempData["Success"] = $"Tag \"{vm.Name}\" updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Route("Tags/{id:int}/Delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _tags.DeleteTagAsync(token, id);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Tag deleted." : result?.Error ?? "Cannot delete — tag may be in use.";
            return RedirectToAction(nameof(Index));
        }
    }
}