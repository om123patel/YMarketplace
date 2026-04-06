using Payments.Domain.Entities;
using Shared.Application.Interfaces;

namespace Payments.Application.Interfaces
{
    public interface ICommissionRuleRepository : IRepository<CommissionRule, int>
    {
        Task<CommissionRule?> GetByCategoryIdAsync(
            int categoryId, CancellationToken ct = default);

        Task<CommissionRule?> GetDefaultAsync(
            CancellationToken ct = default);

        Task<IEnumerable<CommissionRule>> GetAllActiveAsync(
            CancellationToken ct = default);

        Task<IEnumerable<CommissionRule>> GetAllAsync(
            CancellationToken ct = default);
    }
}