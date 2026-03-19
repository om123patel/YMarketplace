using AdminPanel.Dtos.Auth;
using AdminPanel.Dtos.Common;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class UserApiClient : ApiClientBase, IUserApiClient
    {
        public UserApiClient(HttpClient http, ILogger<UserApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<PagedResult<UserDto>>?> GetUsersAsync(
            string token, int page = 1, int pageSize = 20,
            string? search = null, string? role = null,
            string? status = null,
            string? sortBy = null, string? sortDirection = null)
        {
            var q = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["search"] = search,
                ["role"] = role,
                ["status"] = status,
                ["sortBy"] = sortBy,
                ["sortDirection"] = sortDirection
            });
            return GetAsync<ApiResponse<PagedResult<UserDto>>>(
                $"api/admin/users{q}", token);
        }

        public Task<ApiResponse<UserDto>?> GetUserByIdAsync(string token, Guid id)
            => GetAsync<ApiResponse<UserDto>>(
                $"api/admin/users/{id}", token);

        public Task<ApiResponse?> SuspendUserAsync(
            string token, Guid id, string reason)
            => PatchAsync<ApiResponse>(
                $"api/admin/users/{id}/suspend", new { reason }, token);

        public Task<ApiResponse?> ActivateUserAsync(string token, Guid id)
            => PatchAsync<ApiResponse>(
                $"api/admin/users/{id}/activate", null, token);

        public Task<ApiResponse?> DeleteUserAsync(string token, Guid id)
            => DeleteAsync<ApiResponse>(
                $"api/admin/users/{id}", token);
    }

}
