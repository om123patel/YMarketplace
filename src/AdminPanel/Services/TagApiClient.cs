// AdminPanel/Services/TagApiClient.cs
using AdminPanel.Dtos.Tags;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class TagApiClient : ApiClientBase, ITagApiClient
    {
        public TagApiClient(HttpClient http, ILogger<TagApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<List<TagDto>>?> GetTagsAsync(string token)
            => GetAsync<ApiResponse<List<TagDto>>>(
                "api/admin/tags", token);

        public Task<ApiResponse<TagDto>?> GetTagByIdAsync(string token, int id)
            => GetAsync<ApiResponse<TagDto>>(
                $"api/admin/tags/{id}", token);

        public Task<ApiResponse<TagDto>?> CreateTagAsync(
            string token, CreateTagRequest request)
            => PostAsync<ApiResponse<TagDto>>(
                "api/admin/tags", request, token);

        public Task<ApiResponse<TagDto>?> UpdateTagAsync(
            string token, int id, UpdateTagRequest request)
            => PutAsync<ApiResponse<TagDto>>(
                $"api/admin/tags/{id}", request, token);

        public Task<ApiResponse?> DeleteTagAsync(string token, int id)
            => DeleteAsync<ApiResponse>(
                $"api/admin/tags/{id}", token);
    }
}