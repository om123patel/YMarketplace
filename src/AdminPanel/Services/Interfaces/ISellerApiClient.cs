// AdminPanel/Services/Interfaces/ISellerApiClient.cs
using AdminPanel.Dtos.Auth;
using AdminPanel.Dtos.Common;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface ISellerApiClient
    {
        Task<ApiResponse<PagedResult<SellerDto>>?> GetSellersAsync(
            string token, int page = 1, int pageSize = 20,
            string? search = null, string? status = null,
            string? sortBy = null, string? sortDirection = null);

        Task<ApiResponse<SellerDto>?> GetSellerByIdAsync(string token, Guid id);
        Task<ApiResponse?> ApproveSellerAsync(string token, Guid id);
        Task<ApiResponse?> RejectSellerAsync(string token, Guid id, string reason);
        Task<ApiResponse?> SuspendSellerAsync(string token, Guid id, string reason);
        Task<ApiResponse?> ActivateSellerAsync(string token, Guid id);
    }
}