using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Orders;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class SellerOrderApiClient : ApiClientBase, ISellerOrderApiClient
    {
        public SellerOrderApiClient(HttpClient http, ILogger<SellerOrderApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<PagedResult<OrderListItemDto>>?> GetMyOrdersAsync(
            string token, int page = 1, int pageSize = 20, string? status = null)
        {
            var q = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["status"] = status
            });
            return GetAsync<ApiResponse<PagedResult<OrderListItemDto>>>(
                $"api/seller/orders{q}", token);
        }

        public Task<ApiResponse<OrderDto>?> GetOrderByIdAsync(string token, Guid id)
            => GetAsync<ApiResponse<OrderDto>>($"api/seller/orders/{id}", token);

        public Task<ApiResponse?> ConfirmOrderAsync(string token, Guid id)
            => PatchAsync<ApiResponse>($"api/seller/orders/{id}/confirm", null, token);

        public Task<ApiResponse?> ShipOrderAsync(
            string token, Guid id, ShipOrderRequest request)
            => PatchAsync<ApiResponse>($"api/seller/orders/{id}/ship", request, token);

        public Task<ApiResponse?> MarkDeliveredAsync(string token, Guid id)
            => PatchAsync<ApiResponse>($"api/seller/orders/{id}/deliver", null, token);
    }
}