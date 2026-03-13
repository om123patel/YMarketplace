using Catalog.Application.DTOs;
using Shared.Application.Models;

namespace Catalog.Application.Services.Interface
{
    public interface ICategoryService
    {
        Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Result<IEnumerable<CategoryDto>>> GetAllActiveAsync(CancellationToken ct = default);
        Task<Result<IEnumerable<CategoryDto>>> GetRootCategoriesAsync(CancellationToken ct = default);
        Task<Result<IEnumerable<CategoryDto>>> GetChildrenAsync(int parentId, CancellationToken ct = default);
        Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, Guid adminId, CancellationToken ct = default);
        Task<Result<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto, Guid adminId, CancellationToken ct = default);
        Task<Result> DeleteAsync(int id, Guid adminId, CancellationToken ct = default);
        Task<Result> ActivateAsync(int id, Guid adminId, CancellationToken ct = default);
        Task<Result> DeactivateAsync(int id, Guid adminId, CancellationToken ct = default);
    }
}
