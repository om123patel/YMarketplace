using Microsoft.EntityFrameworkCore;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Shared.Infrastructure.Persistence;

namespace Orders.Infrastructure.Persistence.Repositories
{
    public class CartRepository
        : BaseRepository<Cart, Guid, OrdersDbContext>, ICartRepository
    {
        public CartRepository(OrdersDbContext context) : base(context) { }

        public async Task<Cart?> GetByBuyerIdAsync(
            Guid buyerId, CancellationToken ct = default)
            => await DbSet.FirstOrDefaultAsync(c => c.BuyerId == buyerId, ct);

        public async Task<Cart?> GetByBuyerIdWithItemsAsync(
            Guid buyerId, CancellationToken ct = default)
            => await DbSet
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.BuyerId == buyerId, ct);
    }
}
