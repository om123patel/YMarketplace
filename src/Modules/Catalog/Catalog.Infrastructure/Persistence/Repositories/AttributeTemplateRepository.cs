using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    public class AttributeTemplateRepository
    : BaseRepository<AttributeTemplate, int, CatalogDbContext>,
      IAttributeTemplateRepository
    {
        public AttributeTemplateRepository(CatalogDbContext context) : base(context) { }

        public async Task<AttributeTemplate?> GetByIdWithItemsAsync(
            int id, CancellationToken ct = default)
            => await DbSet
                .Include(t => t.Items.OrderBy(i => i.SortOrder))
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<AttributeTemplate?> GetByCategoryIdAsync(
            int categoryId, CancellationToken ct = default)
            => await DbSet
                .Include(t => t.Items.OrderBy(i => i.SortOrder))
                .FirstOrDefaultAsync(t => t.CategoryId == categoryId && t.IsActive, ct);

        public async Task<IEnumerable<AttributeTemplate>> GetAllAsync(
            CancellationToken ct = default)
            => await DbSet
                .Include(t => t.Category)
                .Include(t => t.Items.OrderBy(i => i.SortOrder))
                .OrderBy(t => t.Name)
                .ToListAsync(ct);

        public async Task<IEnumerable<AttributeTemplate>> GetAllActiveAsync(
            CancellationToken ct = default)
            => await DbSet
                .Where(t => t.IsActive)
                .Include(t => t.Category)
                .Include(t => t.Items.OrderBy(i => i.SortOrder))
                .OrderBy(t => t.Name)
                .ToListAsync(ct);

        public async Task<bool> ExistsForCategoryAsync(
            int categoryId, CancellationToken ct = default)
            => await DbSet.AnyAsync(t => t.CategoryId == categoryId, ct);

        public async Task<bool> ExistsAsync(
            int id, CancellationToken ct = default)
            => await DbSet.AnyAsync(t => t.Id == id, ct);
    }

}
