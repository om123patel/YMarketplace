using AdminPanel.Dtos.Payments;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface ICommissionRuleApiClient
    {
        Task<ApiResponse<List<CommissionRuleDto>>?> GetAllAsync(string token);

        Task<ApiResponse<CommissionRuleDto>?> GetByIdAsync(string token, int id);

        Task<ApiResponse<CommissionRuleDto>?> CreateAsync(
            string token, CreateCommissionRuleRequest request);

        Task<ApiResponse<CommissionRuleDto>?> UpdateAsync(
            string token, int id, UpdateCommissionRuleRequest request);

        Task<ApiResponse?> ActivateAsync(string token, int id);

        Task<ApiResponse?> DeactivateAsync(string token, int id);

        Task<ApiResponse?> DeleteAsync(string token, int id);
    }
}