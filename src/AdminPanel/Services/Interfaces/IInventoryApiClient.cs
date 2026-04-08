using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Inventory;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface IInventoryApiClient
    {
        Task<ApiResponse<PagedResult<StockListItemDto>>?> GetPagedAsync(
            string token, int page = 1, int pageSize = 20,
            string? search = null, string? status = null,
            bool? lowStockOnly = null,
            string sortBy = "updatedat", string sortDirection = "desc");

        Task<ApiResponse<List<StockListItemDto>>?> GetLowStockAsync(string token);

        Task<ApiResponse<StockDto>?> GetByProductIdAsync(
            string token, Guid productId);

        Task<ApiResponse<StockDto>?> AddStockAsync(
            string token, Guid productId, AdjustStockRequest request);

        Task<ApiResponse<StockDto>?> SetQuantityAsync(
            string token, Guid productId, AdjustStockRequest request);

        Task<ApiResponse<StockDto>?> UpdateSettingsAsync(
            string token, Guid productId, UpdateStockSettingsRequest request);

        Task<ApiResponse<StockDto>?> CreateStockAsync(
            string token, Guid productId, CreateStockRequest request);
    }
}