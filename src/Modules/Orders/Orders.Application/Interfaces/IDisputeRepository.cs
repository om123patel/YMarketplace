using Orders.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Orders.Application.Interfaces
{
    public interface IDisputeRepository : IRepository<Dispute, Guid>
    {
        Task<PagedList<Dispute>> GetPagedAsync(int page, int pageSize, string? status, CancellationToken ct = default);
        Task<Dispute?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    }
}
