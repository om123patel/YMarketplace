using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Orders;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface ISellerOrderApiClient
    {
        Task<ApiResponse<PagedResult<OrderListItemDto>>?> GetMyOrdersAsync(
            string token, int page = 1, int pageSize = 20, string? status = null);

        Task<ApiResponse<OrderDto>?> GetOrderByIdAsync(string token, Guid id);

        Task<ApiResponse?> ConfirmOrderAsync(string token, Guid id);

        Task<ApiResponse?> ShipOrderAsync(
            string token, Guid id, ShipOrderRequest request);

        Task<ApiResponse?> MarkDeliveredAsync(string token, Guid id);
    }
}