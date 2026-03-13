using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    public class TagRepository
    : BaseRepository<Tag, int, CatalogDbContext>, ITagRepository
    {
        public TagRepository(CatalogDbContext context) : base(context) { }

        public async Task<IEnumerable<Tag>> GetAllAsync(
            CancellationToken ct = default)
            => await DbSet
                .OrderBy(t => t.Name)
                .ToListAsync(ct);

        public async Task<IEnumerable<Tag>> GetByIdsAsync(
            IEnumerable<int> ids, CancellationToken ct = default)
            => await DbSet
                .Where(t => ids.Contains(t.Id))
                .ToListAsync(ct);

        public async Task<bool> ExistsAsync(
            int id, CancellationToken ct = default)
            => await DbSet.AnyAsync(t => t.Id == id, ct);

        public async Task<bool> SlugExistsAsync(
            string slug, CancellationToken ct = default)
            => await DbSet.AnyAsync(t => t.Slug == slug, ct);

        public async Task<bool> NameExistsAsync(
            string name, CancellationToken ct = default)
            => await DbSet.AnyAsync(t => t.Name == name, ct);

        public async Task<bool> SlugExistsExceptAsync(
            string slug, int excludeId, CancellationToken ct = default)
            => await DbSet.AnyAsync(t => t.Slug == slug && t.Id != excludeId, ct);

        public async Task<bool> NameExistsExceptAsync(
            string name, int excludeId, CancellationToken ct = default)
            => await DbSet.AnyAsync(t => t.Name == name && t.Id != excludeId, ct);
    }

}
