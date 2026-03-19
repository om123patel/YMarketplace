using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Products;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface IProductApiClient
    {
        Task<ApiResponse<PagedResult<ProductListItemDto>>?> GetProductsAsync(
            string token, int page, int pageSize,
            string? search = null, string? status = null,
            int? categoryId = null, int? brandId = null,
            string? creatorType = null,
            string sortBy = "createdat",
            string sortDirection = "desc");

        Task<ApiResponse<ProductDto>?> GetProductByIdAsync(string token, Guid id);

        Task<ApiResponse<ProductDto>?> CreateProductAsync(
            string token, CreateProductRequest request);

        Task<ApiResponse<ProductDto>?> UpdateProductAsync(
            string token, Guid id, UpdateProductRequest request);

        Task<ApiResponse?> DeleteProductAsync(string token, Guid id);
        Task<ApiResponse?> ApproveProductAsync(string token, Guid id);
        Task<ApiResponse?> RejectProductAsync(string token, Guid id, string reason);
        Task<ApiResponse?> ArchiveProductAsync(string token, Guid id);
    }
}