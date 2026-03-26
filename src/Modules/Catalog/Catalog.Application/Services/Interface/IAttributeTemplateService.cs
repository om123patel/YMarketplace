using Catalog.Application.DTOs.Attributes;
using Shared.Application.Models;

namespace Catalog.Application.Services.Interface
{
    public interface IAttributeTemplateService
    {
        Task<Result<PagedList<AttributeTemplateDto>>> GetPagedAsync(
            AttributeFilterRequest filter, CancellationToken ct = default);
        Task<Result<IEnumerable<AttributeTemplateDto>>> GetAllAsync(CancellationToken ct = default);
        Task<Result<AttributeTemplateDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Result<AttributeTemplateDto>> GetByCategoryIdAsync(int categoryId, CancellationToken ct = default);
        Task<Result<AttributeTemplateDto>> CreateAsync(CreateAttributeTemplateDto dto, Guid adminId, CancellationToken ct = default);
        Task<Result<AttributeTemplateDto>> UpdateAsync(int id, UpdateAttributeTemplateDto dto, Guid adminId, CancellationToken ct = default);
        Task<Result> DeleteAsync(int id, Guid adminId, CancellationToken ct = default);
    }
}
