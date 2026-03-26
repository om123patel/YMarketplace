using Catalog.Application.DTOs.Products;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    public class ProductRepository
    : BaseRepository<Product, Guid, CatalogDbContext>, IProductRepository
    {
        public ProductRepository(CatalogDbContext context) : base(context) { }

        public async Task<Product?> GetByIdWithDetailsAsync(
            Guid id, CancellationToken ct = default)
            => await DbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Seo)
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .Include(p => p.Tags)
                .Include(p => p.Variants.Where(v => !v.IsDeleted)
                    .OrderBy(v => v.SortOrder))
                    .ThenInclude(v => v.Attributes.OrderBy(a => a.SortOrder))
                .FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<bool> SlugExistsAsync(
            string slug, CancellationToken ct = default)
            => await DbSet.AnyAsync(p => p.Slug == slug, ct);

        public async Task<bool> SlugExistsExceptAsync(
            string slug, Guid excludeProductId, CancellationToken ct = default)
            => await DbSet.AnyAsync(
                p => p.Slug == slug && p.Id != excludeProductId, ct);

        public async Task<bool> SkuExistsAsync(
            string sku, CancellationToken ct = default)
            => await DbSet.AnyAsync(p => p.Sku == sku, ct);

        public async Task<bool> SkuExistsExceptAsync(
            string sku, Guid excludeProductId, CancellationToken ct = default)
            => await DbSet.AnyAsync(
                p => p.Sku == sku && p.Id != excludeProductId, ct);

        public async Task<int> CountByStatusAsync(
            string status, CancellationToken ct = default)
        {
            var parsedStatus = Enum.Parse<ProductStatus>(status);
            return await DbSet.CountAsync(p => p.Status == parsedStatus, ct);
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(
            int categoryId, CancellationToken ct = default)
            => await DbSet
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Brand)
                .Include(p => p.Images.Where(i => i.IsPrimary))
                .ToListAsync(ct);

        public async Task<IEnumerable<Product>> GetBySellerIdAsync(
            Guid sellerId, CancellationToken ct = default)
            => await DbSet
                .Where(p => p.SellerId == sellerId)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images.Where(i => i.IsPrimary))
                .ToListAsync(ct);

        public async Task<PagedList<Product>> GetPagedAsync(
            ProductFilterRequest filter, CancellationToken ct = default)
        {
            var query = DbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images.Where(i => i.IsPrimary))
                .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .AsQueryable();

            // ── Filters ──
            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(p =>
                    p.Name.Contains(filter.Search) ||
                    (p.Sku != null && p.Sku.Contains(filter.Search)));

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                var status = Enum.Parse<ProductStatus>(filter.Status);
                query = query.Where(p => p.Status == status);
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            if (filter.BrandId.HasValue)
                query = query.Where(p => p.BrandId == filter.BrandId.Value);

            if (filter.SellerId.HasValue)
                query = query.Where(p => p.SellerId == filter.SellerId.Value);

            if (filter.IsFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.CreatorType))
            {
                var creatorType = Enum.Parse<ProductCreatorType>(filter.CreatorType);
                query = query.Where(p => p.CreatorType == creatorType);
            }

            if (filter.CreatedFrom.HasValue)
                query = query.Where(p => p.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(p => p.CreatedAt <= filter.CreatedTo.Value);

            // ── Sorting ──
            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortDirection == "desc"
                                    ? query.OrderByDescending(p => p.Name)
                                    : query.OrderBy(p => p.Name),
                "price" => filter.SortDirection == "desc"
                                    ? query.OrderByDescending(p => p.BasePrice.Amount)
                                    : query.OrderBy(p => p.BasePrice.Amount),
                "createdat" => filter.SortDirection == "desc"
                                    ? query.OrderByDescending(p => p.CreatedAt)
                                    : query.OrderBy(p => p.CreatedAt),
                "status" => filter.SortDirection == "desc"
                                    ? query.OrderByDescending(p => p.Status)
                                    : query.OrderBy(p => p.Status),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // ── Pagination ──
            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(ct);

            return new PagedList<Product>(items, filter.Page, filter.PageSize, totalCount);
        }
    }

}
