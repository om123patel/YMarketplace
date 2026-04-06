using Payments.Application.DTOs.CommissionRules;

namespace AdminPanel.Services.Interfaces
{
    public interface ICommissionRuleApiClient
    {
        Task<IEnumerable<CommissionRuleDto>?> GetAllAsync();

        Task<CommissionRuleDto?> GetByIdAsync(int id);

        Task<CommissionRuleDto?> CreateAsync(CreateCommissionRuleDto dto);

        Task<CommissionRuleDto?> UpdateAsync(int id, UpdateCommissionRuleDto dto);

        Task<bool> ActivateAsync(int id);

        Task<bool> DeactivateAsync(int id);

        Task<bool> DeleteAsync(int id);
    }

}
