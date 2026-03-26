using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Tags;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface ITagApiClient
    {
        Task<ApiResponse<PagedResult<TagDto>>?> GetPaged(string token,
       int page, int pageSize,
       string? search = null,
       string? status = null,
       string sortBy = "name",
       string sortDirection = "asc");
        Task<ApiResponse<List<TagDto>>?> GetTagsAsync(string token);
        Task<ApiResponse<TagDto>?> GetTagByIdAsync(string token, int id);
        Task<ApiResponse<TagDto>?> CreateTagAsync(
            string token, CreateTagRequest request);
        Task<ApiResponse<TagDto>?> UpdateTagAsync(
            string token, int id, UpdateTagRequest request);
        Task<ApiResponse?> DeleteTagAsync(string token, int id);
    }
}