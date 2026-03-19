// AdminPanel/Services/AttributeApiClient.cs
using AdminPanel.Dtos.Attributes;
using AdminPanel.Dtos.Categories;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class AttributeApiClient : ApiClientBase, IAttributeApiClient
    {
        public AttributeApiClient(HttpClient http, ILogger<AttributeApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<List<AttributeTemplateDto>>?> GetAttributeTemplatesAsync(
            string token)
            => GetAsync<ApiResponse<List<AttributeTemplateDto>>>(
                "api/admin/attribute-templates", token);

        public Task<ApiResponse<AttributeTemplateDto>?> GetAttributeTemplateByIdAsync(
            string token, int id)
            => GetAsync<ApiResponse<AttributeTemplateDto>>(
                $"api/admin/attribute-templates/{id}", token);

        public Task<ApiResponse<AttributeTemplateDto>?> CreateAttributeTemplateAsync(
            string token, CreateAttributeTemplateRequest request)
            => PostAsync<ApiResponse<AttributeTemplateDto>>(
                "api/admin/attribute-templates", request, token);

        public Task<ApiResponse<AttributeTemplateDto>?> UpdateAttributeTemplateAsync(
            string token, int id, UpdateAttributeTemplateRequest request)
            => PutAsync<ApiResponse<AttributeTemplateDto>>(
                $"api/admin/attribute-templates/{id}", request, token);

        public Task<ApiResponse?> DeleteAttributeTemplateAsync(string token, int id)
            => DeleteAsync<ApiResponse>(
                $"api/admin/attribute-templates/{id}", token);

        public Task<ApiResponse<List<CategoryDto>>?> GetCategoriesAsync(string token)
            => GetAsync<ApiResponse<List<CategoryDto>>>(
                "api/admin/categories", token);
    }
}