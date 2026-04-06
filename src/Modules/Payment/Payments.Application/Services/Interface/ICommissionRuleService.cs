using Payments.Application.DTOs.CommissionRules;
using Shared.Application.Models;

namespace Payments.Application.Services.Interface
{
    public interface ICommissionRuleService
    {
        Task<Result<IEnumerable<CommissionRuleDto>>> GetAllAsync(
            CancellationToken ct = default);

        Task<Result<CommissionRuleDto>> GetByIdAsync(
            int id, CancellationToken ct = default);

        Task<Result<decimal>> CalculateCommissionAsync(
            decimal amount, int? categoryId,
            CancellationToken ct = default);

        Task<Result<CommissionRuleDto>> CreateAsync(
            CreateCommissionRuleDto dto,
            Guid adminId, CancellationToken ct = default);

        Task<Result<CommissionRuleDto>> UpdateAsync(
            int id, UpdateCommissionRuleDto dto,
            Guid adminId, CancellationToken ct = default);

        Task<Result> ActivateAsync(
            int id, Guid adminId, CancellationToken ct = default);

        Task<Result> DeactivateAsync(
            int id, Guid adminId, CancellationToken ct = default);

        Task<Result> DeleteAsync(
            int id, Guid adminId, CancellationToken ct = default);
    }
}