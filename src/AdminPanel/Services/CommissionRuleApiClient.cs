using AdminPanel.Dtos.Payments;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class CommissionRuleApiClient : ApiClientBase, ICommissionRuleApiClient
    {
        public CommissionRuleApiClient(
            HttpClient http, ILogger<CommissionRuleApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<List<CommissionRuleDto>>?> GetAllAsync(string token)
            => GetAsync<ApiResponse<List<CommissionRuleDto>>>(
                "api/admin/commission-rules", token);

        public Task<ApiResponse<CommissionRuleDto>?> GetByIdAsync(
            string token, int id)
            => GetAsync<ApiResponse<CommissionRuleDto>>(
                $"api/admin/commission-rules/{id}", token);

        public Task<ApiResponse<CommissionRuleDto>?> CreateAsync(
            string token, CreateCommissionRuleRequest request)
            => PostAsync<ApiResponse<CommissionRuleDto>>(
                "api/admin/commission-rules", request, token);

        public Task<ApiResponse<CommissionRuleDto>?> UpdateAsync(
            string token, int id, UpdateCommissionRuleRequest request)
            => PutAsync<ApiResponse<CommissionRuleDto>>(
                $"api/admin/commission-rules/{id}", request, token);

        public Task<ApiResponse?> ActivateAsync(string token, int id)
            => PostAsync<ApiResponse>(
                $"api/admin/commission-rules/{id}/activate", null, token);

        public Task<ApiResponse?> DeactivateAsync(string token, int id)
            => PostAsync<ApiResponse>(
                $"api/admin/commission-rules/{id}/deactivate", null, token);

        public Task<ApiResponse?> DeleteAsync(string token, int id)
            => DeleteAsync<ApiResponse>(
                $"api/admin/commission-rules/{id}", token);
    }
}