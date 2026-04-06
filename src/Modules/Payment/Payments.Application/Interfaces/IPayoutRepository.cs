using Payments.Application.DTOs.Payouts;
using Payments.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Payments.Application.Interfaces
{
    public interface IPayoutRepository : IRepository<Payout, Guid>
    {
        Task<PagedList<Payout>> GetPagedAsync(
            PayoutFilterRequest filter,
            CancellationToken ct = default);

        Task<IEnumerable<Payout>> GetBySellerIdAsync(
            Guid sellerId, CancellationToken ct = default);

        Task<bool> HasPendingPayoutAsync(
            Guid sellerId, CancellationToken ct = default);
    }
}