using Catalog.Domain.Entities;

namespace Catalog.Application.Interfaces
{
    public interface IProductStatusHistoryRepository
    {
        Task AddAsync(ProductStatusHistory entry, CancellationToken ct = default);
        Task<IEnumerable<ProductStatusHistory>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
        Task<ProductStatusHistory?> GetLatestByProductIdAsync(Guid productId, CancellationToken ct = default);
    }
}
