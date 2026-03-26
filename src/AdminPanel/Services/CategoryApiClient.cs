// AdminPanel/Services/CategoryApiClient.cs
using AdminPanel.Dtos.Categories;
using AdminPanel.Dtos.Common;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class CategoryApiClient : ApiClientBase, ICategoryApiClient
    {
        public CategoryApiClient(HttpClient http, ILogger<CategoryApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<List<CategoryDto>>?> GetCategoriesAsync(string token)
            => GetAsync<ApiResponse<List<CategoryDto>>>(
                "api/admin/categories", token);

        public Task<ApiResponse<CategoryDto>?> GetCategoryByIdAsync(
            string token, int id)
            => GetAsync<ApiResponse<CategoryDto>>(
                $"api/admin/categories/{id}", token);

        public Task<ApiResponse<CategoryDto>?> CreateCategoryAsync(
            string token, CreateCategoryRequest request)
            => PostAsync<ApiResponse<CategoryDto>>(
                "api/admin/categories", request, token);

        public Task<ApiResponse<CategoryDto>?> UpdateCategoryAsync(
            string token, int id, UpdateCategoryRequest request)
            => PutAsync<ApiResponse<CategoryDto>>(
                $"api/admin/categories/{id}", request, token);

        public Task<ApiResponse?> DeleteCategoryAsync(string token, int id)
            => DeleteAsync<ApiResponse>(
                $"api/admin/categories/{id}", token);

        public Task<ApiResponse?> ToggleCategoryActiveAsync(
            string token, int id, bool activate)
            => PatchAsync<ApiResponse>(
                $"api/admin/categories/{id}/{(activate ? "activate" : "deactivate")}",
                null, token);

        public async Task<ApiResponse<PagedResult<CategoryDto>>?> GetPaged(string token, 
            int page, int pageSize, string? search = null, 
            string? status = null, string sortBy = "name", 
            string sortDirection = "asc")
        {
            var q = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["search"] = search,
                ["status"] = status,
                ["sortBy"] = sortBy,
                ["sortDirection"] = sortDirection
            });
            return await GetAsync<ApiResponse<PagedResult<CategoryDto>>>(
                $"api/admin/categories/GetPaged{q}", token);
        }
    }
}