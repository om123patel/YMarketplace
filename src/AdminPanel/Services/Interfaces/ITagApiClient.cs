// AdminPanel/Services/Interfaces/ITagApiClient.cs
using AdminPanel.Dtos.Tags;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface ITagApiClient
    {
        Task<ApiResponse<List<TagDto>>?> GetTagsAsync(string token);
        Task<ApiResponse<TagDto>?> GetTagByIdAsync(string token, int id);
        Task<ApiResponse<TagDto>?> CreateTagAsync(
            string token, CreateTagRequest request);
        Task<ApiResponse<TagDto>?> UpdateTagAsync(
            string token, int id, UpdateTagRequest request);
        Task<ApiResponse?> DeleteTagAsync(string token, int id);
    }
}