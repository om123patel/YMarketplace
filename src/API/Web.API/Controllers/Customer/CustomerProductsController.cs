// src/API/Web.API/Controllers/Customer/CustomerProductsController.cs

using Catalog.Application.DTOs.Products;
using Catalog.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Models;

namespace Web.API.Controllers.Customer
{
    [Route("api/products")]
    public class CustomerProductsController : BaseController
    {
        private readonly IProductService _productService;

        public CustomerProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET api/products?page=1&pageSize=20&categoryId=1&search=phone&sortBy=price&sortDirection=asc
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProducts(
            [FromQuery] CustomerProductFilterRequest filter,
            CancellationToken ct)
        {
            var adminFilter = new ProductFilterRequest
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                Search = filter.Search,
                CategoryId = filter.CategoryId,
                BrandId = filter.BrandId,
                Status = "Active",      // Buyers only see active products
                IsActive = true,
                SortBy = filter.SortBy,
                SortDirection = filter.SortDirection
            };

            var result = await _productService.GetPagedAsync(adminFilter, ct);
            return HandleResult(result);
        }

        // GET api/products/{id}
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _productService.GetByIdAsync(id, ct);
            if (result.IsSuccess && result.Value!.Status != "Active")
                return NotFound();
            return HandleResult(result);
        }

        // GET api/products/slug/{slug}
        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySlug(
            string slug,
            [FromServices] ICustomerProductQueryService queryService,
            CancellationToken ct)
        {
            var result = await queryService.GetBySlugAsync(slug, ct);
            return HandleResult(result);
        }

        // GET api/products/featured?limit=10
        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeatured(
            [FromServices] ICustomerProductQueryService queryService,
            int limit = 10,
            CancellationToken ct = default)
        {
            var result = await queryService.GetFeaturedAsync(limit, ct);
            return HandleResult(result);
        }

        // GET api/products/top-selling?limit=10
        [HttpGet("top-selling")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopSelling(
            [FromServices] ICustomerProductQueryService queryService,
            int limit = 10,
            CancellationToken ct = default)
        {
            var result = await queryService.GetTopSellingAsync(limit, ct);
            return HandleResult(result);
        }

        // GET api/products/{id}/related?limit=8
        [HttpGet("{id:guid}/related")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRelated(
            Guid id,
            [FromServices] ICustomerProductQueryService queryService,
            int limit = 8,
            CancellationToken ct = default)
        {
            var result = await queryService.GetRelatedAsync(id, limit, ct);
            return HandleResult(result);
        }

        // GET api/products/recommended?limit=10  (personalised if logged in)
        [HttpGet("recommended")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecommended(
            [FromServices] ICustomerProductQueryService queryService,
            int limit = 10,
            CancellationToken ct = default)
        {
            var result = await queryService.GetRecommendedAsync(limit, ct);
            return HandleResult(result);
        }

        // GET api/products/by-category/{categoryId}?page=1&pageSize=20
        [HttpGet("by-category/{categoryId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(
            int categoryId,
            int page = 1, int pageSize = 20,
            string? sortBy = null, string sortDirection = "asc",
            CancellationToken ct = default)
        {
            var filter = new ProductFilterRequest
            {
                Page = page,
                PageSize = pageSize,
                CategoryId = categoryId,
                Status = "Active",
                IsActive = true,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var result = await _productService.GetPagedAsync(filter, ct);
            return HandleResult(result);
        }

        // GET api/products/search?q=iphone&page=1&pageSize=20
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search(
            [FromQuery] string q,
            int page = 1, int pageSize = 20,
            int? categoryId = null, int? brandId = null,
            string? sortBy = null, string sortDirection = "asc",
            CancellationToken ct = default)
        {
            var filter = new ProductFilterRequest
            {
                Page = page,
                PageSize = pageSize,
                Search = q,
                CategoryId = categoryId,
                BrandId = brandId,
                Status = "Active",
                IsActive = true,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var result = await _productService.GetPagedAsync(filter, ct);
            return HandleResult(result);
        }
    }

    // Lightweight filter for customer-facing product browsing
   
}