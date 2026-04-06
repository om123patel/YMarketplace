using Microsoft.EntityFrameworkCore;
using Payments.Application.DTOs.Payouts;
using Payments.Application.Interfaces;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Shared.Application.Models;
using Shared.Infrastructure.Persistence;

namespace Payments.Infrastructure.Persistence.Repositories
{
    public class PayoutRepository
        : BaseRepository<Payout, Guid, PaymentsDbContext>,
          IPayoutRepository
    {
        public PayoutRepository(PaymentsDbContext ctx) : base(ctx) { }

        public async Task<IEnumerable<Payout>> GetBySellerIdAsync(
            Guid sellerId, CancellationToken ct = default)
            => await DbSet
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);

        public async Task<bool> HasPendingPayoutAsync(
            Guid sellerId, CancellationToken ct = default)
            => await DbSet.AnyAsync(
                p => p.SellerId == sellerId &&
                     (p.Status == PayoutStatus.Pending ||
                      p.Status == PayoutStatus.Processing),
                ct);

        public async Task<PagedList<Payout>> GetPagedAsync(
            PayoutFilterRequest filter,
            CancellationToken ct = default)
        {
            var q = DbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                Enum.TryParse<PayoutStatus>(filter.Status, out var status))
                q = q.Where(p => p.Status == status);

            if (filter.SellerId.HasValue)
                q = q.Where(p => p.SellerId == filter.SellerId.Value);

            if (filter.DateFrom.HasValue)
                q = q.Where(p => p.CreatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                q = q.Where(p => p.CreatedAt <= filter.DateTo.Value);

            q = q.OrderByDescending(p => p.CreatedAt);

            var total = await q.CountAsync(ct);
            var items = await q
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(ct);

            return new PagedList<Payout>(items, filter.Page, filter.PageSize, total);
        }
    }
}