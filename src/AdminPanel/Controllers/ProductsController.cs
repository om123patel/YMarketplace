using AdminPanel.Dtos.Products;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class ProductsController : Controller
    {
        private readonly IApiClient _api;
        private readonly AuthTokenService _tokens;

        public ProductsController(IApiClient api, AuthTokenService tokens)
        {
            _api = api;
            _tokens = tokens;
        }

        // ══════════════════════════════════════════════════════════
        // ADMIN PRODUCT LIST
        // GET /Products
        // ══════════════════════════════════════════════════════════

        public async Task<IActionResult> Index(
            string? search,
            string? status,
            int? categoryId,
            int? brandId,
            string? creatorType,
            string sortBy = "createdat",
            string sortDirection = "desc",
            int page = 1,
            CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";

            var productsTask = _api.GetProductsAsync(
                token, page, 20, search, status, categoryId,
                brandId, creatorType, sortBy, sortDirection);
            var categoriesTask = _api.GetCategoriesAsync(token);
            var brandsTask = _api.GetBrandsAsync(token);

            await Task.WhenAll(productsTask, categoriesTask, brandsTask);

            var result = await productsTask;
            var vm = new ProductListViewModel
            {
                Items = result?.Data?.Items.Select(MapToListItem).ToList() ?? [],
                Page = result?.Data?.Page ?? page,
                PageSize = result?.Data?.PageSize ?? 20,
                TotalCount = result?.Data?.TotalCount ?? 0,
                Search = search,
                Status = status,
                CategoryId = categoryId,
                BrandId = brandId,
                CreatorType = creatorType,
                SortBy = sortBy,
                SortDirection = sortDirection,
                Categories = (await categoriesTask)?.Data?
                    .Select(c => new SelectItem { Id = c.Id, Name = c.Name })
                    .ToList() ?? [],
                Brands = (await brandsTask)?.Data?
                    .Select(b => new SelectItem { Id = b.Id, Name = b.Name })
                    .ToList() ?? []
            };

            return View(vm);
        }

        // ══════════════════════════════════════════════════════════
        // SELLER PRODUCT LIST
        // GET /Products/Seller
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Seller(
            string? search,
            string? status,
            int? categoryId,
            string sortBy = "createdat",
            string sortDirection = "desc",
            int page = 1,
            CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";

            var productsTask = _api.GetSellerProductsAsync(
                token, page, 20, search, status, categoryId,
                sortBy, sortDirection);
            var categoriesTask = _api.GetCategoriesAsync(token);

            await Task.WhenAll(productsTask, categoriesTask);

            var result = await productsTask;
            var vm = new SellerProductListViewModel
            {
                Items = result?.Data?.Items.Select(MapToListItem).ToList() ?? [],
                Page = result?.Data?.Page ?? page,
                PageSize = result?.Data?.PageSize ?? 20,
                TotalCount = result?.Data?.TotalCount ?? 0,
                Search = search,
                Status = status,
                CategoryId = categoryId,
                SortBy = sortBy,
                SortDirection = sortDirection,
                Categories = (await categoriesTask)?.Data?
                    .Select(c => new SelectItem { Id = c.Id, Name = c.Name })
                    .ToList() ?? []
            };

            return View(vm);
        }

        // ══════════════════════════════════════════════════════════
        // ADMIN PRODUCT DETAIL
        // GET /Products/{id}
        // ══════════════════════════════════════════════════════════

        [HttpGet("{id:guid}", Name = "ProductDetail")]
        [Route("Products/{id:guid}")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.GetProductByIdAsync(token, id);

            if (result?.Data is null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            var p = result.Data;
            var vm = MapToDetailVm(p, isAdmin: true);
            return View(vm);
        }

        // ══════════════════════════════════════════════════════════
        // SELLER PRODUCT DETAIL
        // GET /Products/SellerDetail/{id}
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        [Route("Products/SellerDetail/{id:guid}")]
        public async Task<IActionResult> SellerDetail(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.GetSellerProductByIdAsync(token, id);

            if (result?.Data is null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Seller));
            }

            var vm = MapToDetailVm(result.Data, isAdmin: false);
            return View("Detail", vm);  // reuse same view
        }

        // ══════════════════════════════════════════════════════════
        // CREATE
        // GET  /Products/Create
        // POST /Products/Create
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var vm = new ProductFormViewModel { IsAdminMode = true };
            await PopulateFormDropdowns(vm, token);
            return View("Form", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ProductFormViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var token2 = _tokens.GetAccessToken() ?? "";
                await PopulateFormDropdowns(vm, token2);
                return View("Form", vm);
            }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.CreateProductAsync(token, BuildCreateRequest(vm));

            if (result?.Success != true)
            {
                ModelState.AddModelError("", result?.Error ?? "Failed to create product.");
                await PopulateFormDropdowns(vm, token);
                return View("Form", vm);
            }

            TempData["Success"] = $"Product '{vm.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════
        // EDIT
        // GET  /Products/{id}/Edit
        // POST /Products/{id}/Edit
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        [Route("Products/{id:guid}/Edit")]
        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.GetProductByIdAsync(token, id);

            if (result?.Data is null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            var vm = MapToFormVm(result.Data, isAdmin: true);
            await PopulateFormDropdowns(vm, token);
            return View("Form", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Products/{id:guid}/Edit")]
        public async Task<IActionResult> Edit(
            Guid id, ProductFormViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var token2 = _tokens.GetAccessToken() ?? "";
                await PopulateFormDropdowns(vm, token2);
                return View("Form", vm);
            }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.UpdateProductAsync(token, id, BuildUpdateRequest(vm));

            if (result?.Success != true)
            {
                ModelState.AddModelError("", result?.Error ?? "Failed to update product.");
                await PopulateFormDropdowns(vm, token);
                return View("Form", vm);
            }

            TempData["Success"] = $"Product '{vm.Name}' updated successfully.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        // ══════════════════════════════════════════════════════════
        // DELETE
        // POST /Products/{id}/Delete
        // ══════════════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Products/{id:guid}/Delete")]
        public async Task<IActionResult> Delete(
            Guid id, string? returnUrl, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.DeleteProductAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Product deleted."
                    : result?.Error ?? "Failed to delete product.";

            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════
        // APPROVAL FLOW (Admin-only actions)
        // ══════════════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Products/{id:guid}/Approve")]
        public async Task<IActionResult> Approve(
            Guid id, string? returnUrl, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.ApproveProductAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Product approved and is now live."
                    : result?.Error ?? "Failed to approve product.";

            return RedirectBack(returnUrl, nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Products/{id:guid}/Reject")]
        public async Task<IActionResult> Reject(
            Guid id, string reason, string? returnUrl, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "A rejection reason is required.";
                return RedirectBack(returnUrl, nameof(Index));
            }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.RejectProductAsync(token, id, reason);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Product rejected."
                    : result?.Error ?? "Failed to reject product.";

            return RedirectBack(returnUrl, nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Products/{id:guid}/Archive")]
        public async Task<IActionResult> Archive(
            Guid id, string? returnUrl, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _api.ArchiveProductAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Product archived."
                    : result?.Error ?? "Failed to archive product.";

            return RedirectBack(returnUrl, nameof(Index));
        }

        // ══════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════

        private static ProductListItem MapToListItem(ProductDto p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Sku = p.Sku ?? "-",
            CategoryName = p.CategoryName ?? "-",
            BrandName = p.BrandName,
            BasePrice = p.BasePrice,
            CurrencyCode = p.CurrencyCode,
            Status = p.Status,
            IsActive = p.IsActive,
            IsFeatured = p.IsFeatured,
            SellerName = p.SellerName,
            PrimaryImageUrl = p.PrimaryImageUrl,
            VariantCount = p.Variants?.Count ?? 0,
            CreatedAt = p.CreatedAt
        };

        private static ProductDetailViewModel MapToDetailVm(
            ProductDto p, bool isAdmin)
        {
            var isPending = p.Status == "PendingApproval";
            var isActive = p.Status == "Active";
            var isDeleted = p.Status == "Deleted";

            return new ProductDetailViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Sku = p.Sku,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                BasePrice = p.BasePrice,
                CurrencyCode = p.CurrencyCode,
                Status = p.Status,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                CategoryName = p.CategoryName ?? "-",
                BrandName = p.BrandName,
                SellerName = p.SellerName,
                PrimaryImageUrl = p.PrimaryImageUrl,
                CreatedAt = p.CreatedAt,
                Images = p.Images.Select(i => new ProductImageItem
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    IsPrimary = i.IsPrimary,
                    SortOrder = i.SortOrder
                }).ToList(),
                Variants = p.Variants.Select(v => new ProductVariantItem
                {
                    Id = v.Id,
                    Name = v.Name,
                    Price = v.Price,
                    Sku = v.Sku,
                    StockQuantity = v.StockQuantity
                }).ToList(),

                // Action availability
                CanApprove = isAdmin && isPending,
                CanReject = isAdmin && isPending,
                CanArchive = isAdmin && isActive,
                CanDelete = isAdmin && !isDeleted,
                CanEdit = !isDeleted
            };
        }

        private static ProductFormViewModel MapToFormVm(ProductDto p, bool isAdmin)
            => new()
            {
                Id = p.Id,
                IsAdminMode = isAdmin,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                BasePrice = p.BasePrice,
                CurrencyCode = p.CurrencyCode,
                Sku = p.Sku,
                IsFeatured = p.IsFeatured,
                Status = p.Status
            };

        private static CreateProductRequest BuildCreateRequest(
            ProductFormViewModel vm) => new()
            {
                CategoryId = vm.CategoryId,
                BrandId = vm.BrandId,
                Name = vm.Name,
                Slug = vm.Slug,
                ShortDescription = vm.ShortDescription,
                Description = vm.Description,
                BasePrice = vm.BasePrice,
                CurrencyCode = vm.CurrencyCode,
                CompareAtPrice = vm.CompareAtPrice,
                CostPrice = vm.CostPrice,
                Sku = vm.Sku,
                Barcode = vm.Barcode,
                WeightKg = vm.WeightKg,
                LengthCm = vm.LengthCm,
                WidthCm = vm.WidthCm,
                HeightCm = vm.HeightCm,
                IsDigital = vm.IsDigital,
                Seo = vm.MetaTitle is not null ? new ProductSeoRequest
                {
                    MetaTitle = vm.MetaTitle,
                    MetaDescription = vm.MetaDescription,
                    MetaKeywords = vm.MetaKeywords,
                    CanonicalUrl = vm.CanonicalUrl,
                    OgTitle = vm.OgTitle,
                    OgDescription = vm.OgDescription,
                    OgImageUrl = vm.OgImageUrl
                } : null,
                TagIds = vm.TagIds ?? [],
                ImageUrls = []
            };

        private static UpdateProductRequest BuildUpdateRequest(
            ProductFormViewModel vm) => new()
            {
                CategoryId = vm.CategoryId,
                BrandId = vm.BrandId,
                Name = vm.Name,
                Slug = vm.Slug,
                ShortDescription = vm.ShortDescription,
                Description = vm.Description,
                BasePrice = vm.BasePrice,
                CurrencyCode = vm.CurrencyCode,
                CompareAtPrice = vm.CompareAtPrice,
                CostPrice = vm.CostPrice,
                Sku = vm.Sku,
                Barcode = vm.Barcode,
                WeightKg = vm.WeightKg,
                LengthCm = vm.LengthCm,
                WidthCm = vm.WidthCm,
                HeightCm = vm.HeightCm,
                IsDigital = vm.IsDigital,
                Seo = vm.MetaTitle is not null ? new ProductSeoRequest
                {
                    MetaTitle = vm.MetaTitle,
                    MetaDescription = vm.MetaDescription,
                    MetaKeywords = vm.MetaKeywords,
                    CanonicalUrl = vm.CanonicalUrl,
                    OgTitle = vm.OgTitle,
                    OgDescription = vm.OgDescription,
                    OgImageUrl = vm.OgImageUrl
                } : null,
                TagIds = vm.TagIds ?? []
            };

        private async Task PopulateFormDropdowns(
            ProductFormViewModel vm, string token)
        {
            var catTask = _api.GetCategoriesAsync(token);
            var brandTask = _api.GetBrandsAsync(token);
            var tagTask = _api.GetTagsAsync(token);

            await Task.WhenAll(catTask, brandTask, tagTask);

            vm.Categories = (await catTask)?.Data?
                .Select(c => new SelectItem { Id = c.Id, Name = c.Name })
                .ToList() ?? [];
            vm.Brands = (await brandTask)?.Data?
                .Select(b => new SelectItem { Id = b.Id, Name = b.Name })
                .ToList() ?? [];
            vm.AvailableTags = (await tagTask)?.Data?
                .Select(t => new TagSelectItem
                {
                    Id = t.Id,
                    Name = t.Name,
                    Selected = vm.TagIds.Contains(t.Id)
                })
                .ToList() ?? [];
        }

        private IActionResult RedirectBack(string? returnUrl, string fallbackAction)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(fallbackAction);
        }
    }
}