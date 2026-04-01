using Orders.Domain.Entities;
using Shared.Application.Interfaces;

namespace Orders.Application.Interfaces
{
    public interface ICartRepository : IRepository<Cart, Guid>
    {
        Task<Cart?> GetByBuyerIdAsync(Guid buyerId, CancellationToken ct = default);
        Task<Cart?> GetByBuyerIdWithItemsAsync(Guid buyerId, CancellationToken ct = default);
    }
}
