using Catalog.Application.DTOs.Tags;
using Shared.Application.Models;

namespace Catalog.Application.Services.Interface
{
    public interface ITagService
    {
        Task<Result<PagedList<TagDto>>> GetPagedAsync(
            TagFilterRequest filter, CancellationToken ct = default);
        Task<Result<IEnumerable<TagDto>>> GetAllAsync(CancellationToken ct = default);
        Task<Result<TagDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Result<TagDto>> CreateAsync(CreateTagDto dto, Guid adminId, CancellationToken ct = default);
        Task<Result<TagDto>> UpdateAsync(int id, UpdateTagDto dto, Guid adminId, CancellationToken ct = default);
        Task<Result> DeleteAsync(int id, Guid adminId, CancellationToken ct = default);
    }
}
