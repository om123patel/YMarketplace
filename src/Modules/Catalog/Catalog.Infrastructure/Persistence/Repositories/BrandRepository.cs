using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
    }

}
