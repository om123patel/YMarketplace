using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Orders;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface IOrderApiClient
    {
        Task<ApiResponse<PagedResult<OrderListItemDto>>?> GetOrdersAsync(
            string token, int page = 1, int pageSize = 20,
            string? status = null, string? paymentStatus = null,
            string? search = null,
            string sortBy = "createdat", string sortDirection = "desc");

        Task<ApiResponse<OrderDto>?> GetOrderByIdAsync(string token, Guid id);

        Task<ApiResponse?> CancelOrderAsync(
            string token, Guid id, CancelOrderRequest request);

        Task<ApiResponse?> ForceRefundAsync(string token, Guid id);

        Task<ApiResponse<PagedResult<DisputeDto>>?> GetDisputesAsync(
            string token, int page = 1, int pageSize = 20, string? status = null);

        Task<ApiResponse<DisputeDto>?> GetDisputeByIdAsync(string token, Guid id);

        Task<ApiResponse?> ResolveDisputeAsync(
            string token, Guid id, ResolveDisputeRequest request);

        Task<ApiResponse?> EscalateDisputeAsync(
            string token, Guid id, EscalateDisputeRequest request);
    }
}