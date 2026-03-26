using Catalog.Application.DTOs.Categories;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    public class CategoryRepository
    : BaseRepository<Category, int, CatalogDbContext>, ICategoryRepository
    {
        public CategoryRepository(CatalogDbContext context) : base(context) { }

        public async Task<IEnumerable<Category>> GetAllActiveAsync(
            CancellationToken ct = default)
            => await DbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(ct);

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync(
            CancellationToken ct = default)
            => await DbSet
                .Where(c => c.ParentId == null && c.IsActive)
                .Include(c => c.Children.Where(ch => ch.IsActive && !ch.IsDeleted))
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(ct);

        public async Task<IEnumerable<Category>> GetChildrenAsync(
            int parentId, CancellationToken ct = default)
            => await DbSet
                .Where(c => c.ParentId == parentId)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(ct);

        public async Task<Category?> GetByIdWithChildrenAsync(
            int id, CancellationToken ct = default)
            => await DbSet
                .Include(c => c.Children.Where(ch => !ch.IsDeleted))
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

        public async Task<bool> ExistsAsync(
            int id, CancellationToken ct = default)
            => await DbSet.AnyAsync(c => c.Id == id, ct);

        public async Task<bool> SlugExistsAsync(
            string slug, CancellationToken ct = default)
            => await DbSet.AnyAsync(c => c.Slug == slug, ct);

        public async Task<bool> SlugExistsExceptAsync(
            string slug, int excludeId, CancellationToken ct = default)
            => await DbSet.AnyAsync(c => c.Slug == slug && c.Id != excludeId, ct);

        public async Task<bool> HasChildrenAsync(
            int id, CancellationToken ct = default)
            => await DbSet.AnyAsync(c => c.ParentId == id, ct);

        public async Task<bool> HasProductsAsync(
            int id, CancellationToken ct = default)
            => await Context.Products.AnyAsync(p => p.CategoryId == id, ct);

        public async Task<PagedList<Category>> GetPagedAsync(CategoryFilterRequest filter, CancellationToken ct = default)
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

            return new PagedList<Category>(items, filter.Page, filter.PageSize, totalCount);
        }
    }

}
