using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Orders;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class OrderApiClient : ApiClientBase, IOrderApiClient
    {
        public OrderApiClient(HttpClient http, ILogger<OrderApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<PagedResult<OrderListItemDto>>?> GetOrdersAsync(
            string token, int page = 1, int pageSize = 20,
            string? status = null, string? paymentStatus = null,
            string? search = null,
            string sortBy = "createdat", string sortDirection = "desc")
        {
            var q = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["status"] = status,
                ["paymentStatus"] = paymentStatus,
                ["search"] = search,
                ["sortBy"] = sortBy,
                ["sortDirection"] = sortDirection
            });
            return GetAsync<ApiResponse<PagedResult<OrderListItemDto>>>(
                $"api/admin/orders{q}", token);
        }

        public Task<ApiResponse<OrderDto>?> GetOrderByIdAsync(string token, Guid id)
            => GetAsync<ApiResponse<OrderDto>>($"api/admin/orders/{id}", token);

        public Task<ApiResponse?> CancelOrderAsync(
            string token, Guid id, CancelOrderRequest request)
            => PatchAsync<ApiResponse>($"api/admin/orders/{id}/cancel", request, token);

        public Task<ApiResponse?> ForceRefundAsync(string token, Guid id)
            => PatchAsync<ApiResponse>($"api/admin/orders/{id}/refund", null, token);

        public Task<ApiResponse<PagedResult<DisputeDto>>?> GetDisputesAsync(
            string token, int page = 1, int pageSize = 20, string? status = null)
        {
            var q = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["status"] = status
            });
            return GetAsync<ApiResponse<PagedResult<DisputeDto>>>(
                $"api/admin/orders/disputes{q}", token);
        }

        public Task<ApiResponse<DisputeDto>?> GetDisputeByIdAsync(string token, Guid id)
            => GetAsync<ApiResponse<DisputeDto>>(
                $"api/admin/orders/disputes/{id}", token);

        public Task<ApiResponse?> ResolveDisputeAsync(
            string token, Guid id, ResolveDisputeRequest request)
            => PatchAsync<ApiResponse>(
                $"api/admin/orders/disputes/{id}/resolve", request, token);

        public Task<ApiResponse?> EscalateDisputeAsync(
            string token, Guid id, EscalateDisputeRequest request)
            => PatchAsync<ApiResponse>(
                $"api/admin/orders/disputes/{id}/escalate", request, token);
    }
}