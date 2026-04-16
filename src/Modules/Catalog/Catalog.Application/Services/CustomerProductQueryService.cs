// src/Modules/Catalog/Catalog.Application/Services/CustomerProductQueryService.cs

using AutoMapper;
using Catalog.Application.DTOs.Products;
using Catalog.Application.Interfaces;
using Catalog.Application.Services.Interface;
using Shared.Application.Models;

namespace Catalog.Application.Services
{
    public class CustomerProductQueryService : ICustomerProductQueryService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public CustomerProductQueryService(
            IProductRepository productRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<ProductDto>> GetBySlugAsync(
            string slug, CancellationToken ct = default)
        {
            var filter = new ProductFilterRequest
            {
                Search = slug,
                Status = "Active",
                IsActive = true,
                Page = 1,
                PageSize = 1
            };

            // Use paged query scoped to active slug match
            var paged = await _productRepository.GetPagedAsync(filter, ct);
            var product = paged.Items.FirstOrDefault(p => p.Slug == slug);

            if (product is null)
                return Result<ProductDto>.Failure(
                    $"Product with slug '{slug}' not found.", "PRODUCT_NOT_FOUND");

            // Reload with full details
            var detail = await _productRepository.GetByIdWithDetailsAsync(product.Id, ct);
            if (detail is null || !detail.IsActive)
                return Result<ProductDto>.Failure(
                    $"Product with slug '{slug}' not found.", "PRODUCT_NOT_FOUND");

            return Result<ProductDto>.Success(_mapper.Map<ProductDto>(detail));
        }

        public async Task<Result<IEnumerable<ProductListItemDto>>> GetFeaturedAsync(
            int limit, CancellationToken ct = default)
        {
            var filter = new ProductFilterRequest
            {
                Page = 1,
                PageSize = Math.Min(limit, 50),
                IsFeatured = true,
                IsActive = true,
                Status = "Active",
                SortBy = "createdat",
                SortDirection = "desc"
            };

            var paged = await _productRepository.GetPagedAsync(filter, ct);
            return Result<IEnumerable<ProductListItemDto>>.Success(
                _mapper.Map<IEnumerable<ProductListItemDto>>(paged.Items));
        }

        public async Task<Result<IEnumerable<ProductListItemDto>>> GetTopSellingAsync(
            int limit, CancellationToken ct = default)
        {
            // Top selling = Active products ordered by creation date (desc) as proxy
            // until Analytics module provides real sales data
            var filter = new ProductFilterRequest
            {
                Page = 1,
                PageSize = Math.Min(limit, 50),
                IsActive = true,
                Status = "Active",
                SortBy = "createdat",
                SortDirection = "desc"
            };

            var paged = await _productRepository.GetPagedAsync(filter, ct);
            return Result<IEnumerable<ProductListItemDto>>.Success(
                _mapper.Map<IEnumerable<ProductListItemDto>>(paged.Items));
        }

        public async Task<Result<IEnumerable<ProductListItemDto>>> GetRelatedAsync(
            Guid productId, int limit, CancellationToken ct = default)
        {
            // Find the source product to get its category
            var product = await _productRepository.GetByIdAsync(productId, ct);
            if (product is null || product.IsDeleted)
                return Result<IEnumerable<ProductListItemDto>>.Success(
                    Enumerable.Empty<ProductListItemDto>());

            // Get active products in same category, excluding the source product
            var filter = new ProductFilterRequest
            {
                Page = 1,
                PageSize = Math.Min(limit + 1, 50),
                CategoryId = product.CategoryId,
                IsActive = true,
                Status = "Active",
                SortBy = "createdat",
                SortDirection = "desc"
            };

            var paged = await _productRepository.GetPagedAsync(filter, ct);
            var related = paged.Items
                .Where(p => p.Id != productId)
                .Take(limit);

            return Result<IEnumerable<ProductListItemDto>>.Success(
                _mapper.Map<IEnumerable<ProductListItemDto>>(related));
        }

        public async Task<Result<IEnumerable<ProductListItemDto>>> GetRecommendedAsync(
            int limit, CancellationToken ct = default)
        {
            // Recommended = featured active products (until ML/Analytics module)
            var filter = new ProductFilterRequest
            {
                Page = 1,
                PageSize = Math.Min(limit, 50),
                IsActive = true,
                Status = "Active",
                IsFeatured = true,
                SortBy = "createdat",
                SortDirection = "desc"
            };

            var paged = await _productRepository.GetPagedAsync(filter, ct);

            // If no featured products, fall back to newest active
            if (!paged.Items.Any())
            {
                filter.IsFeatured = null;
                paged = await _productRepository.GetPagedAsync(filter, ct);
            }

            return Result<IEnumerable<ProductListItemDto>>.Success(
                _mapper.Map<IEnumerable<ProductListItemDto>>(paged.Items));
        }
    }
}