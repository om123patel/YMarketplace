using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    public class ProductStatusHistoryRepository : IProductStatusHistoryRepository
    {
        private readonly CatalogDbContext _context;

        public ProductStatusHistoryRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            ProductStatusHistory entry, CancellationToken ct = default)
            => await _context.ProductStatusHistories.AddAsync(entry, ct);

        public async Task<IEnumerable<ProductStatusHistory>> GetByProductIdAsync(
            Guid productId, CancellationToken ct = default)
            => await _context.ProductStatusHistories
                .Where(h => h.ProductId == productId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync(ct);

        public async Task<ProductStatusHistory?> GetLatestByProductIdAsync(
            Guid productId, CancellationToken ct = default)
            => await _context.ProductStatusHistories
                .Where(h => h.ProductId == productId)
                .OrderByDescending(h => h.ChangedAt)
                .FirstOrDefaultAsync(ct);
    }

}
