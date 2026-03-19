using AdminPanel.Dtos.Products;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Common;
using AdminPanel.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("products")]
    public class ProductsController : Controller
    {
        private readonly AuthTokenService _tokenService;
        private readonly IProductApiClient _products;
        private readonly ICategoryApiClient _categories;
        private readonly IBrandApiClient _brands;
        private readonly ITagApiClient _tags;
        

        public ProductsController(IProductApiClient products,
    ICategoryApiClient categories,
    IBrandApiClient brands,
    ITagApiClient tags,
    AuthTokenService tokenService)
        {
            _products = products;
            _categories = categories;
            _brands = brands;
            _tags = tags;
            _tokenService = tokenService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
    string? search, string? status,
    int? categoryId, int? brandId,
    string? creatorType,
    string sortBy = "createdat",
    string sortDirection = "desc",
    int page = 1,
    CancellationToken ct = default)
        {
            var token = _tokenService.GetAccessToken() ?? "";

            var resultTask = _products.GetProductsAsync(token, page, 20, search, status,
                                     categoryId, brandId, creatorType, sortBy, sortDirection);
            var categoriesTask = _categories.GetCategoriesAsync(token);
            var brandsTask = _brands.GetBrandsAsync(token);

            await Task.WhenAll(resultTask, categoriesTask, brandsTask);

            var result = await resultTask;
            var categories = await categoriesTask;
            var brands = await brandsTask;

            var vm = new ProductListViewModel
            {
                Items = result?.Data?.Items.Select(p => new ProductListItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    Sku = p.Sku ?? "-",
                    CategoryName = p.CategoryName,
                    BasePrice = p.BasePrice,
                    CurrencyCode = p.CurrencyCode,
                    Status = p.Status,
                    SellerName = p.SellerName,
                    PrimaryImageUrl = p.PrimaryImageUrl,
                    IsFeatured = p.IsFeatured,
                    //VariantCount = p.VariantCount,
                    //CreatorType = p.CreatorType,
                    CreatedAt = p.CreatedAt
                }).ToList() ?? [],
                Page = result?.Data?.Page ?? 1,
                PageSize = result?.Data?.PageSize ?? 20,
                TotalCount = result?.Data?.TotalCount ?? 0,
                Search = search,
                StatusFilter = status,
                CategoryId = categoryId,
                BrandId = brandId,
                CreatorType = creatorType,
                SortBy = sortBy,
                SortDirection = sortDirection,
                Categories = categories?.Data?.Select(c =>
                    new FilterOption { Value = c.Id.ToString(), Label = c.Name }).ToList() ?? [],
                Brands = brands?.Data?.Select(b =>
                    new FilterOption { Value = b.Id.ToString(), Label = b.Name }).ToList() ?? []
            };

            return View(vm);
        }

        // GET /products/create
        [HttpGet("create")]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new CreateProductViewModel { IsAdminMode = true };
            await PopulateDropdowns(vm);
            return View("Form", vm);
        }

        // GET /products/{id}/edit
        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var token = _tokenService.GetAccessToken() ?? "";
            var result = await _products.GetProductByIdAsync(token, id);

            if (result?.Success != true || result.Data is null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            var p = result.Data;
            var vm = new CreateProductViewModel
            {
                Id = p.Id,
                IsAdminMode = true,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                BasePrice = p.BasePrice,
                CurrencyCode = p.CurrencyCode,
                CompareAtPrice = p.CompareAtPrice,
                Sku = p.Sku,
                Barcode = p.Barcode,
                IsDigital = p.IsDigital,
                WeightKg = p.WeightKg,
                IsFeatured = p.IsFeatured,
                Status = p.Status,
                RejectionReason = p.RejectionReason,
                SelectedTagIds = p.Tags.Select(t => t.Id).ToList(),
                MetaTitle = p.Seo?.MetaTitle,
                MetaDescription = p.Seo?.MetaDescription,
                MetaKeywords = p.Seo?.MetaKeywords,
                CanonicalUrl = p.Seo?.CanonicalUrl,
                OgTitle = p.Seo?.OgTitle,
                OgDescription = p.Seo?.OgDescription,
                OgImageUrl = p.Seo?.OgImageUrl,
                ExistingImages = p.Images.Select(i => new ExistingImageItem
                {
                    Id = i.Id,
                    Url = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                }).ToList(),
                Variants = p.Variants.Select(v => new ExistingVariantItem
                {
                    Id = v.Id,
                    Name = v.Name,
                    Price = v.Price,
                    CurrencyCode = v.CurrencyCode,
                    CompareAtPrice = v.CompareAtPrice,
                    CostPrice = null,
                    Sku = v.Sku,
                    Barcode = v.Barcode,
                    IsActive = v.IsActive,
                    Attributes = v.Attributes.Select(a => new ExistingAttributeItem
                    {
                        Name = a.Name,
                        Value = a.Value,
                        SortOrder = a.SortOrder
                    }).ToList()
                }).ToList()
            };

            await PopulateDropdowns(vm);
            return View("Form", vm);
        }

        // GET /products/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var token = _tokenService.GetAccessToken() ?? "";
            var result = await _products.GetProductByIdAsync(token, id);

            if (result?.Success != true || result.Data is null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            var p = result.Data;
            var vm = new ProductDetailViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                CategoryName = p.CategoryName,
                BrandName = p.BrandName,
                BasePrice = p.BasePrice,
                CurrencyCode = p.CurrencyCode,
                CompareAtPrice = p.CompareAtPrice,
                Sku = p.Sku,
                Barcode = p.Barcode,
                WeightKg = p.WeightKg,
                IsDigital = p.IsDigital,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                Status = p.Status,
                RejectionReason = p.RejectionReason,
                // CreatorType = p.CreatorType,
                SellerId = p.SellerId,
                SellerName = p.SellerName,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                MetaTitle = p.Seo?.MetaTitle,
                MetaDescription = p.Seo?.MetaDescription,
                MetaKeywords = p.Seo?.MetaKeywords,
                CanonicalUrl = p.Seo?.CanonicalUrl,
                OgTitle = p.Seo?.OgTitle,
                OgDescription = p.Seo?.OgDescription,
                OgImageUrl = p.Seo?.OgImageUrl,
                Images = p.Images.Select(i => new ProductDetailImageItem
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    IsPrimary = i.IsPrimary,
                    SortOrder = i.SortOrder
                }).ToList(),
                Variants = p.Variants.Select(v => new ProductDetailVariantItem
                {
                    Id = v.Id,
                    Name = v.Name,
                    Sku = v.Sku,
                    Price = v.Price,
                    CurrencyCode = v.CurrencyCode,
                    CompareAtPrice = v.CompareAtPrice,
                    IsActive = v.IsActive,
                    Attributes = v.Attributes.Select(a => new ProductDetailAttributeItem
                    {
                        Name = a.Name,
                        Value = a.Value
                    }).ToList()
                }).ToList(),
                Tags = p.Tags.Select(t => new ProductDetailTagItem
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug
                }).ToList()
            };

            return View(vm);
        }

        // POST /products/save
        [HttpPost("save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
            CreateProductViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(vm);
                return View("Form", vm);
            }

            var token = _tokenService.GetAccessToken() ?? "";
            var seo = BuildSeoRequest(vm);

            if (vm.IsEditMode)
            {
                var result = await _products.UpdateProductAsync(
                    token, vm.Id!.Value, new UpdateProductRequest
                    {
                        Name = vm.Name,
                        Slug = vm.Slug,
                        ShortDescription = vm.ShortDescription,
                        Description = vm.Description,
                        CategoryId = vm.CategoryId,
                        BrandId = vm.BrandId,
                        BasePrice = vm.BasePrice,
                        CurrencyCode = vm.CurrencyCode,
                        CompareAtPrice = vm.CompareAtPrice,
                        Sku = vm.Sku,
                        Barcode = vm.Barcode,
                        IsDigital = vm.IsDigital,
                        WeightKg = vm.WeightKg,
                        TagIds = vm.SelectedTagIds,
                        Seo = seo
                    });

                if (result?.Success != true)
                {
                    ModelState.AddModelError("", result?.Error ?? "Failed to update product.");
                    await PopulateDropdowns(vm);
                    return View("Form", vm);
                }

                TempData["Success"] = $"Product '{vm.Name}' updated successfully.";
                return RedirectToAction(nameof(Detail), new { id = vm.Id });
            }
            else
            {
                var result = await _products.CreateProductAsync(
                    token, new CreateProductRequest
                    {
                        Name = vm.Name,
                        Slug = vm.Slug,
                        ShortDescription = vm.ShortDescription,
                        Description = vm.Description,
                        CategoryId = vm.CategoryId,
                        BrandId = vm.BrandId,
                        BasePrice = vm.BasePrice,
                        CurrencyCode = vm.CurrencyCode,
                        CompareAtPrice = vm.CompareAtPrice,
                        Sku = vm.Sku,
                        IsDigital = vm.IsDigital,
                        TagIds = vm.SelectedTagIds,
                        Seo = seo
                    });

                if (result?.Success != true)
                {
                    ModelState.AddModelError("", result?.Error ?? "Failed to create product.");
                    await PopulateDropdowns(vm);
                    return View("Form", vm);
                }

                TempData["Success"] = $"Product '{vm.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST /products/{id}/approve
        [HttpPost("{id:guid}/approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
        {
            var token = _tokenService.GetAccessToken() ?? "";
            var result = await _products.ApproveProductAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Product approved successfully."
                    : result?.Error ?? "Failed to approve product.";

            return RedirectToAction(nameof(Index));
        }

        // POST /products/{id}/reject
        [HttpPost("{id:guid}/reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            Guid id, string reason, CancellationToken ct)
        {
            var token = _tokenService.GetAccessToken() ?? "";
            var result = await _products.RejectProductAsync(token, id, reason);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Product rejected."
                    : result?.Error ?? "Failed to reject product.";

            return RedirectToAction(nameof(Index));
        }

        // POST /products/{id}/archive
        [HttpPost("{id:guid}/archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
        {
            var token = _tokenService.GetAccessToken() ?? "";
            var result = await _products.ArchiveProductAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Product archived."
                    : result?.Error ?? "Failed to archive product.";

            return RedirectToAction(nameof(Index));
        }

        // POST /products/{id}/delete
        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var token = _tokenService.GetAccessToken() ?? "";
            await _products.DeleteProductAsync(token, id);
            TempData["Success"] = "Product deleted.";
            return RedirectToAction(nameof(Index));
        }

        // ── Private ────────────────────────────────────────────
        private async Task PopulateDropdowns(CreateProductViewModel vm)
        {
            var token = _tokenService.GetAccessToken() ?? "";
            var categories = await _categories.GetCategoriesAsync(token);
            var brands = await _brands.GetBrandsAsync(token);
            var tags = await _tags.GetTagsAsync(token);

            vm.Categories = categories?.Data?.Select(c =>
                new SelectItem { Id = c.Id, Name = c.Name }).ToList() ?? [];

            vm.Brands = brands?.Data?.Select(b =>
                new SelectItem { Id = b.Id, Name = b.Name }).ToList() ?? [];

            vm.AvailableTags = tags?.Data?.Select(t =>
                new SelectItem
                {
                    Id = t.Id,
                    Name = t.Name,
                    Selected = vm.SelectedTagIds.Contains(t.Id)
                }).ToList() ?? [];
        }

        private static ProductSeoRequest BuildSeoRequest(CreateProductViewModel vm)
            => new()
            {
                MetaTitle = vm.MetaTitle,
                MetaDescription = vm.MetaDescription,
                MetaKeywords = vm.MetaKeywords,
                CanonicalUrl = vm.CanonicalUrl,
                OgTitle = vm.OgTitle,
                OgDescription = vm.OgDescription,
                OgImageUrl = vm.OgImageUrl
            };

        [HttpGet("seller")]
        public async Task<IActionResult> Seller(
    string? search, string? status,
    int? categoryId,
    string sortBy = "createdat",
    string sortDirection = "desc",
    int page = 1,
    CancellationToken ct = default)
        {
            var token = _tokenService.GetAccessToken() ?? "";

            var resultTask = _products.GetProductsAsync(
                                    token, page, 20, search, status,
                                    categoryId, brandId: null,
                                    creatorType: "Seller",   // always filter to Seller only
                                    sortBy, sortDirection);
            var categoriesTask = _categories.GetCategoriesAsync(token);

            await Task.WhenAll(resultTask, categoriesTask);

            var result = await resultTask;
            var categories = await categoriesTask;

            var vm = new SellerProductListViewModel
            {
                Items = result?.Data?.Items.Select(p => new SellerProductListItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    Sku = p.Sku,
                    CategoryName = p.CategoryName,
                    BasePrice = p.BasePrice,
                    CurrencyCode = p.CurrencyCode,
                    Status = p.Status,
                    SellerName = p.SellerName,
                    PrimaryImageUrl = p.PrimaryImageUrl,
                    VariantCount = p.VariantCount,
                    CreatedAt = p.CreatedAt
                }).ToList() ?? [],
                Page = result?.Data?.Page ?? 1,
                PageSize = result?.Data?.PageSize ?? 20,
                TotalCount = result?.Data?.TotalCount ?? 0,
                Search = search,
                StatusFilter = status,
                CategoryId = categoryId,
                SortBy = sortBy,
                SortDirection = sortDirection,
                Categories = categories?.Data?.Select(c =>
                    new FilterOption { Value = c.Id.ToString(), Label = c.Name }).ToList() ?? []
            };

            return View(vm);
        }
    }
}