using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Payments;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class PayoutApiClient : ApiClientBase, IPayoutApiClient
    {
        public PayoutApiClient(HttpClient http, ILogger<PayoutApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<PagedResult<PayoutListItemDto>>?> GetPayoutsAsync(
            string token, int page = 1, int pageSize = 20,
            string? status = null, string? search = null)
        {
            var q = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["status"] = status,
                ["search"] = search
            });
            return GetAsync<ApiResponse<PagedResult<PayoutListItemDto>>>(
                $"api/admin/payouts{q}", token);
        }

        public Task<ApiResponse<PayoutDto>?> GetByIdAsync(string token, Guid id)
            => GetAsync<ApiResponse<PayoutDto>>(
                $"api/admin/payouts/{id}", token);

        public Task<ApiResponse<PayoutDto>?> StartProcessingAsync(
            string token, Guid id, string? note)
            => PostAsync<ApiResponse<PayoutDto>>(
                $"api/admin/payouts/{id}/process", new { note }, token);

        public Task<ApiResponse<PayoutDto>?> CompleteAsync(
            string token, Guid id, ProcessPayoutRequest request)
            => PostAsync<ApiResponse<PayoutDto>>(
                $"api/admin/payouts/{id}/complete", request, token);

        public Task<ApiResponse<PayoutDto>?> FailAsync(
            string token, Guid id, string reason)
            => PostAsync<ApiResponse<PayoutDto>>(
                $"api/admin/payouts/{id}/fail", new { reason }, token);

        public Task<ApiResponse<PayoutDto>?> CancelAsync(
            string token, Guid id, string reason)
            => PostAsync<ApiResponse<PayoutDto>>(
                $"api/admin/payouts/{id}/cancel", new { reason }, token);
    }
}