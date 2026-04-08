using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Inventory;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class InventoryApiClient : ApiClientBase, IInventoryApiClient
    {
        public InventoryApiClient(
            HttpClient http, ILogger<InventoryApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<PagedResult<StockListItemDto>>?> GetPagedAsync(
            string token, int page = 1, int pageSize = 20,
            string? search = null, string? status = null,
            bool? lowStockOnly = null,
            string sortBy = "updatedat", string sortDirection = "desc")
        {
            var q = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["search"] = search,
                ["status"] = status,
                ["lowStockOnly"] = lowStockOnly?.ToString().ToLower(),
                ["sortBy"] = sortBy,
                ["sortDirection"] = sortDirection
            });
            return GetAsync<ApiResponse<PagedResult<StockListItemDto>>>(
                $"api/admin/inventory{q}", token);
        }

        public Task<ApiResponse<List<StockListItemDto>>?> GetLowStockAsync(
            string token)
            => GetAsync<ApiResponse<List<StockListItemDto>>>(
                "api/admin/inventory/low-stock", token);

        public Task<ApiResponse<StockDto>?> GetByProductIdAsync(
            string token, Guid productId)
            => GetAsync<ApiResponse<StockDto>>(
                $"api/admin/inventory/product/{productId}", token);

        public Task<ApiResponse<StockDto>?> AddStockAsync(
            string token, Guid productId, AdjustStockRequest request)
            => PatchAsync<ApiResponse<StockDto>>(
                $"api/admin/inventory/product/{productId}/add", request, token);

        public Task<ApiResponse<StockDto>?> SetQuantityAsync(
            string token, Guid productId, AdjustStockRequest request)
            => PatchAsync<ApiResponse<StockDto>>(
                $"api/admin/inventory/product/{productId}/set", request, token);

        public Task<ApiResponse<StockDto>?> UpdateSettingsAsync(
            string token, Guid productId, UpdateStockSettingsRequest request)
            => PatchAsync<ApiResponse<StockDto>>(
                $"api/admin/inventory/product/{productId}/settings", request, token);

        public Task<ApiResponse<StockDto>?> CreateStockAsync(
            string token, Guid productId, CreateStockRequest request)
            => PostAsync<ApiResponse<StockDto>>(
                $"api/admin/inventory/product/{productId}", request, token);
    }
}