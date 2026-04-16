// src/Modules/Catalog/Catalog.Application/Interfaces/IWishlistRepository.cs

using Catalog.Domain.Entities;
using Shared.Application.Interfaces;

namespace Catalog.Application.Interfaces
{
    public interface IWishlistRepository : IRepository<Wishlist, Guid>
    {
        Task<Wishlist?> GetByBuyerIdWithItemsAsync(
            Guid buyerId, CancellationToken ct = default);
        Task<bool> ExistsByBuyerIdAsync(
            Guid buyerId, CancellationToken ct = default);
    }
}