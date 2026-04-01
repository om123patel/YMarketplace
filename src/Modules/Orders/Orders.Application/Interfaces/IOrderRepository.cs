using Orders.Application.DTOs.Orders;
using Orders.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Orders.Application.Interfaces
{
    public interface IOrderRepository : IRepository<Order, Guid>
    {
        Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
        Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
        Task<PagedList<Order>> GetPagedAsync(OrderFilterRequest filter, CancellationToken ct = default);
        Task<PagedList<Order>> GetByBuyerIdAsync(Guid buyerId, int page, int pageSize, CancellationToken ct = default);
        Task<PagedList<Order>> GetByStoreIdAsync(Guid storeId, int page, int pageSize, string? status, CancellationToken ct = default);
    }
}
