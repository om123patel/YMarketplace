using Shared.Application.Interfaces;

namespace Inventory.Infrastructure.Persistence.UnitOfWork
{
    public class InventoryUnitOfWork : IUnitOfWork
    {
        private readonly InventoryDbContext _context;

        public InventoryUnitOfWork(InventoryDbContext context)
            => _context = context;

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}