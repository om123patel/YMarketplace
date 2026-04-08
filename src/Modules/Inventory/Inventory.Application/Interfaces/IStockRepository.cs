using Inventory.Application.DTOs;
using Inventory.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Inventory.Application.Interfaces
{
    public interface IStockRepository : IRepository<Stock, Guid>
    {
        Task<Stock?> GetByProductIdAsync(
            Guid productId, CancellationToken ct = default);

        Task<Stock?> GetByProductAndVariantAsync(
            Guid productId, Guid? variantId, CancellationToken ct = default);

        Task<PagedList<Stock>> GetPagedAsync(
            StockFilterRequest filter, CancellationToken ct = default);

        Task<IEnumerable<Stock>> GetLowStockAsync(
            CancellationToken ct = default);

        Task<bool> ExistsForProductAsync(
            Guid productId, CancellationToken ct = default);
    }
}