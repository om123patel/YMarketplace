using AdminPanel.Services.Interfaces;
using Payments.Application.DTOs.CommissionRules;

namespace AdminPanel.Services
{
    public class CommissionRuleApiClient : ICommissionRuleApiClient
    {
        private readonly HttpClient _http;

        public CommissionRuleApiClient(HttpClient http) => _http = http;

        public async Task<IEnumerable<CommissionRuleDto>?> GetAllAsync()
            => await _http.GetFromJsonAsync<IEnumerable<CommissionRuleDto>>(
                "api/admin/commission-rules");

        public async Task<CommissionRuleDto?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<CommissionRuleDto>(
                $"api/admin/commission-rules/{id}");

        public async Task<CommissionRuleDto?> CreateAsync(
            CreateCommissionRuleDto dto)
        {
            var response = await _http.PostAsJsonAsync(
                "api/admin/commission-rules", dto);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<CommissionRuleDto>()
                : null;
        }

        public async Task<CommissionRuleDto?> UpdateAsync(
            int id, UpdateCommissionRuleDto dto)
        {
            var response = await _http.PutAsJsonAsync(
                $"api/admin/commission-rules/{id}", dto);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<CommissionRuleDto>()
                : null;
        }

        public async Task<bool> ActivateAsync(int id)
            => (await _http.PostAsJsonAsync(
                $"api/admin/commission-rules/{id}/activate", (object?)null))
               .IsSuccessStatusCode;

        public async Task<bool> DeactivateAsync(int id)
            => (await _http.PostAsJsonAsync(
                $"api/admin/commission-rules/{id}/deactivate", (object?)null))
               .IsSuccessStatusCode;

        public async Task<bool> DeleteAsync(int id)
            => (await _http.DeleteAsync(
                $"api/admin/commission-rules/{id}"))
               .IsSuccessStatusCode;
    }

}
