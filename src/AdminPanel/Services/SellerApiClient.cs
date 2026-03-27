// AdminPanel/Services/SellerApiClient.cs
using AdminPanel.Dtos.Auth;
using AdminPanel.Dtos.Common;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class SellerApiClient : ApiClientBase, ISellerApiClient
    {
        public SellerApiClient(HttpClient http, ILogger<SellerApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<PagedResult<SellerDto>>?> GetPaged(
            string token, int page = 1, int pageSize = 20,
            string? search = null, string? status = null,
            string? sortBy = null, string? sortDirection = null)
        {
            var q = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["search"] = search,
                ["status"] = status,
                ["sortBy"] = sortBy,
                ["sortDirection"] = sortDirection
            });
            return GetAsync<ApiResponse<PagedResult<SellerDto>>>(
                $"api/admin/sellers/GetPaged{q}", token);
        }

        public Task<ApiResponse<SellerDto>?> GetSellerByIdAsync(
            string token, Guid id)
            => GetAsync<ApiResponse<SellerDto>>(
                $"api/admin/sellers/{id}", token);

        public Task<ApiResponse?> ApproveSellerAsync(string token, Guid id)
            => PatchAsync<ApiResponse>(
                $"api/admin/sellers/{id}/approve", null, token);

        public Task<ApiResponse?> RejectSellerAsync(
            string token, Guid id, string reason)
            => PatchAsync<ApiResponse>(
                $"api/admin/sellers/{id}/reject", new { reason }, token);

        public Task<ApiResponse?> SuspendSellerAsync(
            string token, Guid id, string reason)
            => PatchAsync<ApiResponse>(
                $"api/admin/sellers/{id}/suspend", new { reason }, token);

        public Task<ApiResponse?> ActivateSellerAsync(string token, Guid id)
            => PatchAsync<ApiResponse>(
                $"api/admin/sellers/{id}/activate", null, token);
    }
}