using Microsoft.EntityFrameworkCore;
using Payments.Application.Interfaces;
using Payments.Domain.Entities;
using Shared.Infrastructure.Persistence;

namespace Payments.Infrastructure.Persistence.Repositories
{
    public class CommissionRuleRepository
        : BaseRepository<CommissionRule, int, PaymentsDbContext>,
          ICommissionRuleRepository
    {
        public CommissionRuleRepository(PaymentsDbContext ctx) : base(ctx) { }

        public async Task<CommissionRule?> GetByCategoryIdAsync(
            int categoryId, CancellationToken ct = default)
            => await DbSet.FirstOrDefaultAsync(
                r => r.CategoryId == categoryId && r.IsActive, ct);

        public async Task<CommissionRule?> GetDefaultAsync(
            CancellationToken ct = default)
            => await DbSet.FirstOrDefaultAsync(
                r => r.CategoryId == null && r.IsActive, ct);

        public async Task<IEnumerable<CommissionRule>> GetAllActiveAsync(
            CancellationToken ct = default)
            => await DbSet
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync(ct);

        public async Task<IEnumerable<CommissionRule>> GetAllAsync(
            CancellationToken ct = default)
            => await DbSet
                .OrderBy(r => r.CategoryId == null ? 0 : 1)
                .ThenBy(r => r.Name)
                .ToListAsync(ct);
    }
}