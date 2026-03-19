// AdminPanel/Services/ProductApiClient.cs
using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Products;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class ProductApiClient : ApiClientBase, IProductApiClient
    {
        public ProductApiClient(HttpClient http, ILogger<ProductApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<PagedResult<ProductListItemDto>>?> GetProductsAsync(
            string token, int page, int pageSize,
            string? search = null, string? status = null,
            int? categoryId = null, int? brandId = null,
            string? creatorType = null,
            string sortBy = "createdat",
            string sortDirection = "desc")
        {
            var q = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["search"] = search,
                ["status"] = status,
                ["categoryId"] = categoryId?.ToString(),
                ["brandId"] = brandId?.ToString(),
                ["creatorType"] = creatorType,
                ["sortBy"] = sortBy,
                ["sortDirection"] = sortDirection
            });
            return GetAsync<ApiResponse<PagedResult<ProductListItemDto>>>(
                $"api/admin/products{q}", token);
        }

        public Task<ApiResponse<ProductDto>?> GetProductByIdAsync(
            string token, Guid id)
            => GetAsync<ApiResponse<ProductDto>>(
                $"api/admin/products/{id}", token);

        public Task<ApiResponse<ProductDto>?> CreateProductAsync(
            string token, CreateProductRequest request)
            => PostAsync<ApiResponse<ProductDto>>(
                "api/admin/products", request, token);

        public Task<ApiResponse<ProductDto>?> UpdateProductAsync(
            string token, Guid id, UpdateProductRequest request)
            => PutAsync<ApiResponse<ProductDto>>(
                $"api/admin/products/{id}", request, token);

        public Task<ApiResponse?> DeleteProductAsync(string token, Guid id)
            => DeleteAsync<ApiResponse>(
                $"api/admin/products/{id}", token);

        public Task<ApiResponse?> ApproveProductAsync(string token, Guid id)
            => PatchAsync<ApiResponse>(
                $"api/admin/products/{id}/approve", null, token);

        public Task<ApiResponse?> RejectProductAsync(
            string token, Guid id, string reason)
            => PatchAsync<ApiResponse>(
                $"api/admin/products/{id}/reject", new { reason }, token);

        public Task<ApiResponse?> ArchiveProductAsync(string token, Guid id)
            => PatchAsync<ApiResponse>(
                $"api/admin/products/{id}/archive", null, token);
    }
}