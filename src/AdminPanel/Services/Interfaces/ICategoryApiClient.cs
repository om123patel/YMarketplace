// AdminPanel/Services/Interfaces/ICategoryApiClient.cs
using AdminPanel.Dtos.Categories;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface ICategoryApiClient
    {
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