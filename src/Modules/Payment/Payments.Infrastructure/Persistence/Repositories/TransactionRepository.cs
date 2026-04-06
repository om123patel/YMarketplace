using Microsoft.EntityFrameworkCore;
using Payments.Application.DTOs.Transactions;
using Payments.Application.Interfaces;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Shared.Application.Models;
using Shared.Infrastructure.Persistence;


namespace Payments.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository
        : BaseRepository<Transaction, Guid, PaymentsDbContext>,
          ITransactionRepository
    {
        public TransactionRepository(PaymentsDbContext ctx) : base(ctx) { }

        public async Task<Transaction?> GetByOrderIdAsync(
            Guid orderId, CancellationToken ct = default)
            => await DbSet.FirstOrDefaultAsync(t => t.OrderId == orderId, ct);

        public async Task<IEnumerable<Transaction>> GetCompletedBySellerIdAsync(
            Guid sellerId, CancellationToken ct = default)
            => await DbSet
                .Where(t => t.SellerId == sellerId
                         && t.Status == TransactionStatus.Completed)
                .ToListAsync(ct);

        public async Task<decimal> GetTotalPaidOutBySellerIdAsync(
            Guid sellerId, CancellationToken ct = default)
        {
            // Join PayoutTransactions to find total paid
            var paidOut = await Context.Set<Shared.Domain.Abstractions.Entity<int>>()
                .IgnoreQueryFilters()
                .ToListAsync(ct);

            // Simpler: query via PayoutsDbSet through context
            return await Context.PayoutTransactions
                .Join(Context.Payouts.Where(p =>
                          p.SellerId == sellerId &&
                          p.Status == Payments.Domain.Enums.PayoutStatus.Completed),
                      pt => pt.PayoutId,
                      p => p.Id,
                      (pt, p) => p.Amount)
                .SumAsync(ct);
        }

        public async Task<decimal> GetPendingBalanceBySellerIdAsync(
            Guid sellerId, CancellationToken ct = default)
        {
            var totalEarned = await DbSet
                .Where(t => t.SellerId == sellerId
                         && t.Status == TransactionStatus.Completed)
                .SumAsync(t => t.SellerAmount, ct);

            var totalPaidOut = await GetTotalPaidOutBySellerIdAsync(sellerId, ct);

            return Math.Max(0, totalEarned - totalPaidOut);
        }

        public async Task<bool> ExistsByOrderIdAsync(
            Guid orderId, CancellationToken ct = default)
            => await DbSet.AnyAsync(t => t.OrderId == orderId, ct);

        public async Task<PagedList<Transaction>> GetPagedAsync(
            TransactionFilterRequest filter,
            CancellationToken ct = default)
        {
            var q = DbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                Enum.TryParse<TransactionStatus>(filter.Status, out var status))
                q = q.Where(t => t.Status == status);

            if (!string.IsNullOrWhiteSpace(filter.Method) &&
                Enum.TryParse<PaymentMethod>(filter.Method, out var method))
                q = q.Where(t => t.Method == method);

            if (filter.SellerId.HasValue)
                q = q.Where(t => t.SellerId == filter.SellerId.Value);

            if (filter.BuyerId.HasValue)
                q = q.Where(t => t.BuyerId == filter.BuyerId.Value);

            if (filter.OrderId.HasValue)
                q = q.Where(t => t.OrderId == filter.OrderId.Value);

            if (filter.DateFrom.HasValue)
                q = q.Where(t => t.CreatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                q = q.Where(t => t.CreatedAt <= filter.DateTo.Value);

            q = filter.SortBy?.ToLower() switch
            {
                "amount" => filter.SortDirection == "asc"
                    ? q.OrderBy(t => t.Amount)
                    : q.OrderByDescending(t => t.Amount),
                _ => q.OrderByDescending(t => t.CreatedAt)
            };

            var total = await q.CountAsync(ct);
            var items = await q
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(ct);

            return new PagedList<Transaction>(items, filter.Page, filter.PageSize, total);
        }
    }
}