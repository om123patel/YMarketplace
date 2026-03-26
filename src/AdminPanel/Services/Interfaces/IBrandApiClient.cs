using AdminPanel.Dtos.Brands;
using AdminPanel.Dtos.Common;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface IBrandApiClient
    {
        Task<ApiResponse<PagedResult<BrandDto>>?> GetPaged(string token,
            int page, int pageSize,
            string? search = null,
            string? status = null,
            string sortBy = "name",
            string sortDirection = "asc");

        Task<ApiResponse<List<BrandDto>>?> GetBrandsAsync(string token);
        Task<ApiResponse<BrandDto>?> GetBrandByIdAsync(string token, int id);
        Task<ApiResponse<BrandDto>?> CreateBrandAsync(
            string token, CreateBrandRequest request);
        Task<ApiResponse<BrandDto>?> UpdateBrandAsync(
            string token, int id, UpdateBrandRequest request);
        Task<ApiResponse?> DeleteBrandAsync(string token, int id);
        Task<ApiResponse?> ToggleBrandActiveAsync(
            string token, int id, bool activate);
    }
}