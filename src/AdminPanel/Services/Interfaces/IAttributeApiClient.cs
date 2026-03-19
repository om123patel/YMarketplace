// AdminPanel/Services/Interfaces/IAttributeApiClient.cs
using AdminPanel.Dtos.Attributes;
using AdminPanel.Dtos.Categories;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface IAttributeApiClient
    {
        Task<ApiResponse<List<AttributeTemplateDto>>?> GetAttributeTemplatesAsync(
            string token);
        Task<ApiResponse<AttributeTemplateDto>?> GetAttributeTemplateByIdAsync(
            string token, int id);
        Task<ApiResponse<AttributeTemplateDto>?> CreateAttributeTemplateAsync(
            string token, CreateAttributeTemplateRequest request);
        Task<ApiResponse<AttributeTemplateDto>?> UpdateAttributeTemplateAsync(
            string token, int id, UpdateAttributeTemplateRequest request);
        Task<ApiResponse?> DeleteAttributeTemplateAsync(string token, int id);
        // AttributesController uses this to populate category dropdown
        Task<ApiResponse<List<CategoryDto>>?> GetCategoriesAsync(string token);
    }
}