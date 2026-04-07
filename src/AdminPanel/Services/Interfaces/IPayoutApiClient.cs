using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Payments;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface IPayoutApiClient
    {
        Task<ApiResponse<PagedResult<PayoutListItemDto>>?> GetPayoutsAsync(
            string token, int page = 1, int pageSize = 20,
            string? status = null, string? search = null);

        Task<ApiResponse<PayoutDto>?> GetByIdAsync(string token, Guid id);

        Task<ApiResponse<PayoutDto>?> StartProcessingAsync(
            string token, Guid id, string? note);

        Task<ApiResponse<PayoutDto>?> CompleteAsync(
            string token, Guid id, ProcessPayoutRequest request);

        Task<ApiResponse<PayoutDto>?> FailAsync(
            string token, Guid id, string reason);

        Task<ApiResponse<PayoutDto>?> CancelAsync(
            string token, Guid id, string reason);
    }
}