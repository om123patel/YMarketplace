// src/Modules/Catalog/Catalog.Infrastructure/Persistence/Repositories/WishlistRepository.cs

using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    public class WishlistRepository
        : BaseRepository<Wishlist, Guid, CatalogDbContext>,
          IWishlistRepository
    {
        public WishlistRepository(CatalogDbContext context) : base(context) { }

        public async Task<Wishlist?> GetByBuyerIdWithItemsAsync(
            Guid buyerId, CancellationToken ct = default)
            => await DbSet
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.BuyerId == buyerId, ct);

        public async Task<bool> ExistsByBuyerIdAsync(
            Guid buyerId, CancellationToken ct = default)
            => await DbSet.AnyAsync(w => w.BuyerId == buyerId, ct);
    }
}