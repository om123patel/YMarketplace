using AdminPanel.Dtos.Categories;
using AdminPanel.Dtos.Common;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface ICategoryApiClient
    {
        Task<ApiResponse<PagedResult<CategoryDto>>?> GetPaged(string token, 
            int page, int pageSize,
            string? search = null, 
            string? status = null,
            string sortBy = "name",
            string sortDirection = "asc");

        Task<ApiResponse<List<CategoryDto>>?> GetCategoriesAsync(string token);
        Task<ApiResponse<CategoryDto>?> GetCategoryByIdAsync(string token, int id);
        Task<ApiResponse<CategoryDto>?> CreateCategoryAsync(
            string token, CreateCategoryRequest request);
        Task<ApiResponse<CategoryDto>?> UpdateCategoryAsync(
            string token, int id, UpdateCategoryRequest request);
        Task<ApiResponse?> DeleteCategoryAsync(string token, int id);
        Task<ApiResponse?> ToggleCategoryActiveAsync(
            string token, int id, bool activate);
    }
}