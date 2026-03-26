using AdminPanel.Dtos.Brands;
using AdminPanel.Dtos.Common;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class BrandApiClient : ApiClientBase, IBrandApiClient
    {
        public BrandApiClient(HttpClient http, ILogger<BrandApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<List<BrandDto>>?> GetBrandsAsync(string token)
            => GetAsync<ApiResponse<List<BrandDto>>>(
                "api/admin/brands", token);

        public Task<ApiResponse<BrandDto>?> GetBrandByIdAsync(
            string token, int id)
            => GetAsync<ApiResponse<BrandDto>>(
                $"api/admin/brands/{id}", token);

        public Task<ApiResponse<BrandDto>?> CreateBrandAsync(
            string token, CreateBrandRequest request)
            => PostAsync<ApiResponse<BrandDto>>(
                "api/admin/brands", request, token);

        public Task<ApiResponse<BrandDto>?> UpdateBrandAsync(
            string token, int id, UpdateBrandRequest request)
            => PutAsync<ApiResponse<BrandDto>>(
                $"api/admin/brands/{id}", request, token);

        public Task<ApiResponse?> DeleteBrandAsync(string token, int id)
            => DeleteAsync<ApiResponse>(
                $"api/admin/brands/{id}", token);

        public Task<ApiResponse?> ToggleBrandActiveAsync(
            string token, int id, bool activate)
            => PatchAsync<ApiResponse>(
                $"api/admin/brands/{id}/{(activate ? "activate" : "deactivate")}",
                null, token);

        public async Task<ApiResponse<PagedResult<BrandDto>>?> GetPaged(string token, int page, int pageSize, string? search = null, string? status = null, string sortBy = "name", string sortDirection = "asc")
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
            return await GetAsync<ApiResponse<PagedResult<BrandDto>>>(
                $"api/admin/brands/GetPaged{q}", token);
        }
    }
}