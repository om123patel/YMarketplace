using AdminPanel.Dtos.Auth;
using AdminPanel.Dtos.Common;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface IUserApiClient
    {
        Task<ApiResponse<PagedResult<UserDto>>?> GetUsersAsync(
            string token, int page = 1, int pageSize = 20,
            string? search = null, string? role = null,
            string? status = null,
            string? sortBy = null, string? sortDirection = null);

        Task<ApiResponse<UserDto>?> GetUserByIdAsync(string token, Guid id);

        Task<ApiResponse?> SuspendUserAsync(
            string token, Guid id, string reason);

        Task<ApiResponse?> ActivateUserAsync(string token, Guid id);

        Task<ApiResponse?> DeleteUserAsync(string token, Guid id);
    }

}
