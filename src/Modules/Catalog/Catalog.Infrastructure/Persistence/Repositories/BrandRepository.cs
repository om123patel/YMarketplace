using Catalog.Application.DTOs.Brands;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    public class BrandRepository
    : BaseRepository<Brand, int, CatalogDbContext>, IBrandRepository
    {
        public BrandRepository(CatalogDbContext context) : base(context) { }

        public async Task<IEnumerable<Brand>> GetAllActiveAsync(
            CancellationToken ct = default)
            => await DbSet
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync(ct);

        public async Task<bool> ExistsAsync(
            int id, CancellationToken ct = default)
            => await DbSet.AnyAsync(b => b.Id == id, ct);

        public async Task<bool> SlugExistsAsync(
            string slug, CancellationToken ct = default)
            => await DbSet.AnyAsync(b => b.Slug == slug, ct);

        public async Task<bool> SlugExistsExceptAsync(
            string slug, int excludeId, CancellationToken ct = default)
            => await DbSet.AnyAsync(b => b.Slug == slug && b.Id != excludeId, ct);

        public async Task<bool> NameExistsAsync(
            string name, CancellationToken ct = default)
            => await DbSet.AnyAsync(b => b.Name == name, ct);

        public async Task<bool> NameExistsExceptAsync(
            string name, int excludeId, CancellationToken ct = default)
            => await DbSet.AnyAsync(b => b.Name == name && b.Id != excludeId, ct);

        public async Task<bool> HasProductsAsync(
            int id, CancellationToken ct = default)
            => await Context.Products.AnyAsync(p => p.BrandId == id, ct);

        public async Task<PagedList<Brand>> GetPagedAsync(BrandFilterRequest filter, 
            CancellationToken ct = default)
        {
            var query = DbSet.AsQueryable();

            // ── Filters ──
            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(p =>
                    p.Name.Contains(filter.Search) ||
                    (p.Slug != null && p.Slug.Contains(filter.Search)) ||
                    (p.Description != null && p.Description.Contains(filter.Search)));

            //if (!string.IsNullOrWhiteSpace(filter.Status))
            //{
            //    var status = Enum.Parse<ProductStatus>(filter.Status);
            //    query = query.Where(p => p.Status == status);
            //}



            // ── Sorting ──
            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortDirection == "desc"
                                    ? query.OrderByDescending(p => p.Name)
                                    : query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // ── Pagination ──
            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(ct);

            return new PagedList<Brand>(items, filter.Page, filter.PageSize, totalCount);
        }
    }

}
