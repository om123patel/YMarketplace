using AdminPanel.Services.Interfaces;
using Payments.Application.DTOs.Payouts;
using Shared.Application.Models;

namespace AdminPanel.Services
{
    public class PayoutApiClient : IPayoutApiClient
    {
        private readonly HttpClient _http;

        public PayoutApiClient(HttpClient http) => _http = http;

        public async Task<PagedList<PayoutListItemDto>?> GetPagedAsync(
            PayoutFilterRequest filter)
        {
            var qs = QueryStringHelper.Build(filter);
            return await _http.GetFromJsonAsync<PagedList<PayoutListItemDto>>(
                $"api/admin/payouts?{qs}");
        }

        public async Task<PayoutDto?> GetByIdAsync(Guid id)
            => await _http.GetFromJsonAsync<PayoutDto>(
                $"api/admin/payouts/{id}");

        public async Task<PayoutDto?> StartProcessingAsync(Guid id, string? note)
        {
            var response = await _http.PostAsJsonAsync(
                $"api/admin/payouts/{id}/process", note);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<PayoutDto>()
                : null;
        }

        public async Task<PayoutDto?> CompleteAsync(Guid id, ProcessPayoutDto dto)
        {
            var response = await _http.PostAsJsonAsync(
                $"api/admin/payouts/{id}/complete", dto);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<PayoutDto>()
                : null;
        }

        public async Task<PayoutDto?> FailAsync(Guid id, string reason)
        {
            var response = await _http.PostAsJsonAsync(
                $"api/admin/payouts/{id}/fail", reason);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<PayoutDto>()
                : null;
        }

        public async Task<PayoutDto?> CancelAsync(Guid id, string reason)
        {
            var response = await _http.PostAsJsonAsync(
                $"api/admin/payouts/{id}/cancel", reason);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<PayoutDto>()
                : null;
        }
    }

}
